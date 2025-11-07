/*using Microsoft.AspNetCore.Mvc;
using PromoPilot.Core.Interfaces;
using PromoPilot.Infrastructure.Data; // Assuming AuditLogs is your EF model
using Microsoft.EntityFrameworkCore;

namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly PromoPilotDbContext _context;

        public AuditLogsController(PromoPilotDbContext context)
        {
            _context = context;
        }

        // Get all audit logs.
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return Ok(logs);
        }

        // Get audit logs by entity name.
        [HttpGet("ByEntity/{entityName}")]
        public async Task<IActionResult> GetByEntity(string entityName)
        {
            var logs = await _context.AuditLogs
                .Where(l => l.EntityName == entityName)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return Ok(logs);
        }

        /// Get audit logs by user ID.
        [HttpGet("ByUser/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var logs = await _context.AuditLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return Ok(logs);
        }
    }
}*/