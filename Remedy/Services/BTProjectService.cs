using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Models;
using Remedy.Services.Interfaces;

namespace Remedy.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IFileService _fileService;

        public BTProjectService(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<List<Project>> GetProjectsAsync(int companyId)
        {
            try
            {
                return await _context.Projects!.Include(p => p.Company)
                                               .Include(p => p.ProjectPriority)
                                               .Where(p => p.CompanyId == companyId && !p.Archived)
                                               .ToListAsync();
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
                return await _context.Projects!.Include(p => p.Company)
                                               .Include(p => p.ProjectPriority)
                                               .Where(p => p.CompanyId == companyId && p.Archived)
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

        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects!.Include(p => p.Company)
                                                           .Include(p => p.Tickets)
                                                           .Include(p => p.ProjectPriority)
                                                           .FirstOrDefaultAsync(p => p.Id == projectId);
                return project!;
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
    }
}
