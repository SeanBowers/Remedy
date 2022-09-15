using Remedy.Data;
using Remedy.Models;
using Remedy.Services.Interfaces;

namespace Remedy.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            try
            {
                if (oldTicket == null && newTicket != null)
                {
                    //New Ticket
                    TicketHistory ticketHistory = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = string.Empty,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                        UserId = userId,
                        Description = "New Ticket Added."
                    };

                    try
                    {
                        await _context.AddAsync(ticketHistory);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else
                {
                    //Modified Ticket
                    if (oldTicket!.Title != newTicket!.Title)
                    {
                        TicketHistory ticketHistory = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Title",
                            OldValue = oldTicket.Title,
                            NewValue = newTicket.Title,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = "Title modified."
                        };
                        await _context.AddAsync(ticketHistory);
                    }
                    if (oldTicket!.Description != newTicket!.Description)
                    {
                        TicketHistory ticketHistory = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Description",
                            OldValue = oldTicket.Description,
                            NewValue = newTicket.Description,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = "Description modified."
                        };
                        await _context.AddAsync(ticketHistory);
                    }
                    if (oldTicket!.TicketPriorityId != newTicket!.TicketPriorityId)
                    {
                        TicketHistory ticketHistory = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Priority",
                            OldValue = oldTicket.TicketPriority!.Name,
                            NewValue = newTicket.TicketPriority!.Name,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = "Priority modified."
                        };
                        await _context.AddAsync(ticketHistory);
                    }
                    if (oldTicket!.TicketStatusId != newTicket!.TicketStatusId)
                    {
                        TicketHistory ticketHistory = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Status",
                            OldValue = oldTicket.TicketStatus!.Name,
                            NewValue = newTicket.TicketStatus!.Name,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = "Status modified."
                        };
                        await _context.AddAsync(ticketHistory);
                    }
                    if (oldTicket!.TicketTypeId != newTicket!.TicketTypeId)
                    {
                        TicketHistory ticketHistory = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Type",
                            OldValue = oldTicket.TicketType!.Name,
                            NewValue = newTicket.TicketType!.Name,
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = "Type modified."
                        };
                        await _context.AddAsync(ticketHistory);
                    }
                    if (oldTicket!.DeveloperUserId != newTicket!.DeveloperUserId)
                    {
                        TicketHistory ticketHistory = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Developer",
                            OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                            NewValue = newTicket.DeveloperUser?.FullName ?? "Not Assigned",
                            Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            UserId = userId,
                            Description = "Developer modified."
                        };
                        await _context.AddAsync(ticketHistory);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddHistoryAsync(int ticketId, string model, string userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets!.FindAsync(ticketId);

                string description = model.ToLower().Replace("ticket", "");

                description = $"New {description} added to ticket: {ticket!.Title}";

                TicketHistory ticketHistory = new()
                {
                    TicketId = ticket.Id,
                    PropertyName = model,
                    OldValue = string.Empty,
                    NewValue = string.Empty,
                    Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                    UserId = userId,
                    Description = description
                };
                try
                {
                    await _context.AddAsync(ticketHistory);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            throw new NotImplementedException();
        }
    }
}
