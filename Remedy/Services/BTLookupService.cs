using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Models;
using Remedy.Services.Interfaces;

namespace Remedy.Services
{
    public class BTLookupService : IBTLookupService
    {

        private readonly ApplicationDbContext _context;

        public BTLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities!.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                return await _context.TicketPriorities!.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                return await _context.TicketStatuses!.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus? ticketStatus = await _context.TicketStatuses!.FirstOrDefaultAsync(n => n.Name == statusName);

                return ticketStatus!.Id;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                return await _context.TicketTypes!.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int?> LookupNotificationTypeIdAsync(string typeName)
        {
            try
            {
                NotificationType? notificationType = await _context.NotificationTypes!.FirstOrDefaultAsync(n => n.Name == typeName);

                return notificationType!.Id;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}