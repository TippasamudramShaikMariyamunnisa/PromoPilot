using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.Application.Services
{
    public class AuditLoggingService : IAuditLoggingService
    {
        private readonly IAuditLogRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLoggingService(IAuditLogRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityName, string entityId, object changes)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var log = new AuditLog
            {
                UserId = userId ?? "Unknown",
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Changes = JsonSerializer.Serialize(changes),
                Timestamp = DateTime.UtcNow
            };

            await _repository.LogAsync(log);
        }
    }
}