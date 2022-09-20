using Remedy.Models;

namespace Remedy.Services.Interfaces
{
    public interface IBTLookupService
    {
        public Task<List<TicketPriority>> GetTicketPrioritiesAsync();
        public Task<List<TicketStatus>> GetTicketStatusesAsync();
        public Task<List<TicketType>> GetTicketTypesAsync();
        public Task<List<ProjectPriority>> GetProjectPrioritiesAsync();
        public Task<int?> LookupNotificationTypeIdAsync(string typeName);
        public Task<int?> LookupTicketStatusIdAsync(string statusName);
    }
}
