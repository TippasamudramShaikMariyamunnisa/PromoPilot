using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Core.Constants;
using PromoPilot.Core.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"❌ Validation error: {error.ErrorMessage}");
            }

            return BadRequest(ModelState);
        }

        try
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return BadRequest(new { message = "You are already logged in. Log out to register a new account." });
            }

            var success = await _authService.RegisterAsync(request);

            if (!success)
                return BadRequest(new { message = "Registration failed." });

            return Ok(new
            {
                message = $"User registered successfully with role '{request.Role}'. A confirmation email has been sent to {request.Email}."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An unexpected error occurred.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An unexpected error occurred.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An unexpected error occurred.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var result = await _authService.LogoutAsync(request.RefreshToken);
            if (!result)
                return BadRequest(new { message = "Refresh token not found or already revoked." });

            return Ok(new { message = "Logged out successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An unexpected error occurred.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
}