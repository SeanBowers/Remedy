using Remedy.Models;

namespace Remedy.Services.Interfaces
{
    public interface IBTTicketService
    {
        //Get Project by Company Id
        public Task<List<Ticket>> GetTicketsAsync(int companyId);

        public Task<List<Ticket>> GetProjectTicketsAsync(int projectId);

        public Task<Ticket> GetTicketByIdAsync(int id);

        public Task<bool> AddTicketDeveloperAsync(string userId, int ticketId);
    }
}
