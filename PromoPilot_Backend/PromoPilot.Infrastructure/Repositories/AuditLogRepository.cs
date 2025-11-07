using System.Security.AccessControl;
using System.Threading.Tasks;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using PromoPilot.Infrastructure.Data;

namespace PromoPilot.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly PromoPilotDbContext _context;

        public AuditLogRepository(PromoPilotDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(AuditLog log)
        {
            var entity = new AuditLog
            {
                UserId = log.UserId,
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Changes = log.Changes,
                Timestamp = log.Timestamp
            };

            _context.AuditLogs.Add(entity);
            await _context.SaveChangesAsync();
        }
    }
}