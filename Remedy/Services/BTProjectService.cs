using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Models;
using Remedy.Models.Enums;
using Remedy.Services.Interfaces;

namespace Remedy.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTFileService fileService,
                                  IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _rolesService = rolesService;
        }



        public async Task<List<Project>> GetProjectsAsync(int companyId)
        {
            try
            {
                return await _context.Projects!.Where(p => p.CompanyId == companyId && !p.Archived)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketAttachments)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketComments)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketHistories)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketPriority)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketStatus)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketType)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.DeveloperUser)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.SubmitterUser)
                                               .Include(p => p.Company)
                                               .Include(p => p.Members)
                                               .Include(p => p.ProjectPriority)
                                               .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> projects = (await _context.Users!.Include(u => u.Projects).ThenInclude(p => p.Company)
                                            .Include(u => u.Projects).ThenInclude(p => p.Members)
                                            .Include(u => u.Projects).ThenInclude(p => p.Tickets)
                                            .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(p => p.DeveloperUser)
                                            .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(p => p.SubmitterUser)
                                            .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(p => p.TicketPriority)
                                            .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(p => p.TicketStatus)
                                            .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(p => p.TicketType)
                                            .FirstOrDefaultAsync(u => u.Id == userId)).Projects!.ToList();
                return projects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects!.Where(p => p.Id == projectId)
                                                           .Include(p => p.Tickets!).ThenInclude(p => p.TicketAttachments)
                                                           .Include(p => p.Tickets!).ThenInclude(p => p.TicketComments)
                                                           .Include(p => p.Tickets!).ThenInclude(p => p.TicketHistories)
                                                           .Include(p => p.Tickets!).ThenInclude(p => p.TicketPriority)
                                                           .Include(p => p.Tickets!).ThenInclude(p => p.TicketStatus)
                                                           .Include(p => p.Tickets!).ThenInclude(p => p.TicketType)
                                                           .Include(p => p.Tickets!).ThenInclude(p => p.DeveloperUser)
                                                           .Include(p => p.Tickets!).ThenInclude(p => p.SubmitterUser)
                                                           .Include(p => p.Company)
                                                           .Include(p => p.Members)
                                                           .Include(p => p.ProjectPriority)
                                                           .FirstOrDefaultAsync();
                return project!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsAsync(int companyId)
        {
            try
            {
                return await _context.Projects!.Where(p => p.CompanyId == companyId && p.Archived)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketAttachments)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketComments)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketHistories)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketPriority)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketStatus)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.TicketType)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.DeveloperUser)
                                               .Include(p => p.Tickets!).ThenInclude(p => p.SubmitterUser)
                                               .Include(p => p.Company)
                                               .Include(p => p.Members)
                                               .Include(p => p.ProjectPriority)
                                               .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveProjectAsync(int projectId)
        {
            try
            {
                var project = await GetProjectByIdAsync(projectId);
                project.Archived = true;
                foreach (var tickets in project.Tickets!)
                {
                    tickets.ArchivedByProject = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RestoreProjectAsync(int projectId)
        {
            try
            {
                var project = await GetProjectByIdAsync(projectId);
                project.Archived = false;
                foreach (var tickets in project.Tickets!)
                {
                    tickets.ArchivedByProject = false;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId)!;
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                if(currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }

                try
                {
                    await AddUserToProjectAsync(selectedPM!, projectId);
                    return true;
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

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                var project = await GetProjectByIdAsync(projectId)!;
                foreach (BTUser member in project.Members!)
                {
                    if(await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveUserFromProjectAsync(member, projectId);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BTUser>? GetProjectManagerAsync(int projectId)
        {
            try
            {
                var project = await GetProjectByIdAsync(projectId);
                foreach (var member in project.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }
                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }



        public async Task<bool> AddUserToProjectAsync(BTUser user, int projectId)
        {
            try
            {
                var project = await GetProjectByIdAsync(projectId);

                bool onProject = !project.Members!.Any(m => m.Id == user.Id);

                if (onProject)
                {
                    project.Members!.Add(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveUserFromProjectAsync(BTUser user, int projectId)
        {
            try
            {
                var project = await GetProjectByIdAsync(projectId);

                bool onProject = project.Members!.Any(m => m.Id == user.Id);

                if (onProject)
                {
                    project.Members!.Remove(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;

            }
            catch (Exception)
            {
                throw;
            }
        } 

        public async Task<List<BTUser>>? GetProjectDevelopersAsync(int projectId)
        {
            try
            {
                var project = await GetProjectByIdAsync(projectId);
                List<BTUser> devs = new();
                foreach (var member in project.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.Developer)))
                    {
                        devs.Add(member);
                    }
                    
                }
                return devs;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>>? GetProjectSubmittersAsync(int projectId)
        {
            try
            {
                var project = await GetProjectByIdAsync(projectId);
                List<BTUser> subs = new();
                foreach (var member in project.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.Submitter)))
                    {
                        subs.Add(member);
                    }
                }
                    return subs;
            }
            catch (Exception)
            {

                throw;
            }
        }



        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            try
            {
                return (await GetProjectByIdAsync(projectId)).Members!.Any(m => m.Id == userId);
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            List<Project> projects = await _context.Projects.Include(p => p.ProjectPriority).Where(p => p.CompanyId == companyId && p.ProjectPriority.Name == priority).ToListAsync();

            return projects;
        }
    }
}
