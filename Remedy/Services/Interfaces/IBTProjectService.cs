using Remedy.Models;

namespace Remedy.Services.Interfaces
{
    public interface IBTProjectService
    {
        //Get Project by Company Id
        public Task<List<Project>> GetProjectsAsync(int companyId);

        //Get Archived Project by Company Id
        public Task<List<Project>> GetArchivedProjectsAsync(int companyId);

        //Add Project
        public Task AddProjectAsync(Project project);

        //Get Project by Id
        public Task<Project> GetProjectByIdAsync(int projectId);

        //Update Project
        public Task UpdateProjectAsync(Project project);

        //Archive Project
        public Task ArchiveProjectAsync(int projectId);

        //Restore Project
        public Task RestoreProjectAsync(int projectId);

    }
}
