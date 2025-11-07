using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Core.Constants;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using PromoPilot.Infrastructure.Data;

namespace PromoPilot.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly PromoPilotDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(PromoPilotDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        public async Task<bool> RegisterAsync(RegisterRequest dto)
        {
            if (!RoleConstants.PublicRoles.Contains(dto.Role))
                throw new ArgumentException("You cannot register with this role.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new ArgumentException("This email is already registered.");

            CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            try
            {
                await _emailService.SendEmailAsync(
                    dto.Email,
                    "Welcome to PromoPilot!",
                    $"<h2>Hi {dto.Username},</h2><p>Your account has been successfully registered with role <strong>{dto.Role}</strong>.</p><p>Thank you!</p>"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"📧 Email send failed: {ex.Message}");
                // Optionally log or continue without failing registration
            }


            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = dto.Role,
                RefreshTokens = new List<RefreshToken>()
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the user. Details: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return true;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest dto)
        {
            var user = await _context.Users.Include(u => u.RefreshTokens)
                                           .FirstOrDefaultAsync(u => u.Email == dto.Email);
     
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            // Check lockout
            if (user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.UtcNow)
                throw new UnauthorizedAccessException("Account is locked. Try again later.");
            
            // Validate password
            var hashedPassword = HashPassword(dto.Password, user.PasswordSalt);
            if (!hashedPassword.SequenceEqual(user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 3)
                {
                    user.LockoutEndTime = DateTime.UtcNow.AddMinutes(15); // Lock for 15 minutes
                    await _context.SaveChangesAsync();
                    throw new UnauthorizedAccessException("Account is locked. Try again later.");
                }

                await _context.SaveChangesAsync();
                throw new UnauthorizedAccessException("Invalid credentials.");
            }
            Console.WriteLine($"Stored Hash: {Convert.ToBase64String(user.PasswordHash)}");
            Console.WriteLine($"Input Hash: {Convert.ToBase64String(hashedPassword)}");
            // Reset on success
            user.FailedLoginAttempts = 0;
            user.LockoutEndTime = null;
            await _context.SaveChangesAsync();

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.Include(rt => rt.User)
                                                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (token == null)
                throw new SecurityTokenException("Refresh token is invalid or revoked.");

            if (token.Expires < DateTime.UtcNow)
                throw new SecurityTokenException("Refresh token has expired.");

            token.IsRevoked = true;
            await _context.SaveChangesAsync();

            return await GenerateAuthResponse(token.User);
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
                return false;

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ------------------ Helper Methods ------------------

        private async Task<AuthResponse> GenerateAuthResponse(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var tokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(
                    double.Parse(_config["Jwt:DurationInMinutes"] ?? "60")),
                RefreshTokenExpiry = tokenEntity.Expires,
                Role = user.Role,
                Email = user.Email,
                Username = user.Username
            };
        }

        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_config["Jwt:DurationInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

    }
}