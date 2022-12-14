using Remedy.Models;

namespace Remedy.Services.Interfaces
{
    public interface IBTTicketService
    {
        //Add new ticket
        public Task AddNewTicketAsync(Ticket ticket);
        //Update ticket
        public Task UpdateTicketAsync(Ticket ticket);
        //Archive Ticket
        public Task ArchiveTicketAsync(Ticket ticket);
        //Restore Ticket
        public Task RestoreTicketAsync(Ticket ticket);
        //Add ticket developer
        public Task<bool> AddTicketDeveloperAsync(string userId, int ticketId);


        //Get all company tickets
        public Task<List<Ticket>> GetTicketsAsync(int companyId);
        //Get tickets in a project
        public Task<List<Ticket>> GetProjectTicketsAsync(int projectId);
        //Get all company archived tickets
        public Task<List<Ticket>> GetArchivedTicketsAsync(int companyId);
        //Get all unassigned tickets
        public Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId);
        //Get specific ticket
        public Task<Ticket> GetTicketByIdAsync(int id);
        //Get tickets for users, admin see all, project manager see all project tickets
        //submitter sees tickets they've submitted. developer sees tickets they are assigned.
        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);
        
        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int compandId);


        //Add ticket attachment
        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);
        //Get ticket attachment by id
        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);


        //Add ticket comment
        public Task AddTicketCommentAsync(TicketComment ticketComment);

    }
}
