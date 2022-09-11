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

        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);

        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);
    }
}
