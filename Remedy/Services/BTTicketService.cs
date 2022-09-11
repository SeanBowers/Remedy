using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Models;
using Remedy.Models.Enums;
using Remedy.Services.Interfaces;

namespace Remedy.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;

        public BTTicketService(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IFileService fileService,
                                  IBTRolesService rolesService,
                                  IBTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _rolesService = rolesService;
            _projectService = projectService;
        }

        public async Task<List<Ticket>> GetTicketsAsync(int companyId)
        {
            try
            {
                return await _context.Tickets!
                            .Include(t => t.Project)
                            .ThenInclude(t => t.Company)
                            .Include(t => t.DeveloperUser)
                            .Include(t => t.SubmitterUser)
                            .Include(t => t.TicketPriority)
                            .Include(t => t.TicketStatus)
                            .Include(t => t.TicketType)
                            .Where(t => !t.Archived && !t.ArchivedByProject && t.Project!.CompanyId == companyId)
                            .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsAsync(int projectId)
        {
            try
            {
                return await _context.Tickets!
                            .Where(t => !t.Archived && !t.ArchivedByProject && t.Project!.Id == projectId)
                            .Include(t => t.Project)
                            .ThenInclude(t => t.Company)
                            .Include(t => t.DeveloperUser)
                            .Include(t => t.SubmitterUser)
                            .Include(t => t.TicketPriority)
                            .Include(t => t.TicketStatus)
                            .Include(t => t.TicketType)
                            .OrderByDescending(t => t.ProjectId)
                            .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int id)
        {
            try
            {
                Ticket? ticket = await _context.Tickets!.Where(t => t.Id == id)
                                              .Include(t => t.Project!).ThenInclude(t => t.Company)
                                              .Include(t => t.DeveloperUser)
                                              .Include(t => t.SubmitterUser)
                                              .Include(t => t.TicketPriority)
                                              .Include(t => t.TicketStatus)
                                              .Include(t => t.TicketType)
                                              .FirstOrDefaultAsync();
                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddTicketDeveloperAsync(string userId, int ticketId)
        {
            try
            {
                if (userId != null)
                {
                    Ticket? ticket = await GetTicketByIdAsync(ticketId);
                    ticket.DeveloperUserId = userId;
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            try
            {
                BTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                List<Ticket> tickets = new();

                if (await _rolesService.IsUserInRoleAsync(user!, nameof(BTRoles.Admin)))
                {
                    tickets = (await _projectService.GetProjectsAsync(companyId)).SelectMany(p => p.Tickets!).ToList();
                }
                //else if (await _rolesService.IsUserInRoleAsync(user!, nameof(BTRoles.ProjectManager)))
                //{
                //    tickets = (await _projectService.GetProjectsAsync(companyId)).SelectMany(p => p.Tickets!).ToList();
                //}
                else if (await _rolesService.IsUserInRoleAsync(user!, nameof(BTRoles.Developer)))
                {
                    tickets = (await _projectService.GetProjectsAsync(companyId)).SelectMany(p => p.Tickets!).Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(user!, nameof(BTRoles.Submitter)))
                {
                    tickets = (await _projectService.GetProjectsAsync(companyId)).SelectMany(p => p.Tickets!).Where(t => t.SubmitterUserId == userId).ToList();
                }

                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
