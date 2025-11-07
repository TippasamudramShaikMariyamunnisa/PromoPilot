namespace PromoPilot.Application.Interfaces
{
    public interface IAuditLoggingService
    {
        Task LogAsync(string action, string entity, string entityId, object details);
    }
}