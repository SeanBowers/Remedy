using Remedy.Models;

namespace Remedy.Services.Interfaces
{
    public interface IBTProjectService
    {
        //Get Project by Company Id
        public Task<List<Project>> GetProjectsAsync(int companyId);

        //Get Archived Project by Company Id
        public Task<List<Project>> GetArchivedProjectsAsync(int companyId);

        //Get Project by Id
        public Task<Project> GetProjectByIdAsync(int projectId);

        public Task<List<Project>> GetUserProjectsAsync(string userId);


        //Add Project
        public Task AddProjectAsync(Project project);

        //Update Project
        public Task UpdateProjectAsync(Project project);

        //Archive Project
        public Task ArchiveProjectAsync(int projectId);

        //Restore Project
        public Task RestoreProjectAsync(int projectId);



        //Get Project Manager for specific project
        public Task<BTUser>? GetProjectManagerAsync(int projectId);

        public Task<List<BTUser>>? GetProjectDevelopersAsync(int projectId);

        public Task<List<BTUser>>? GetProjectSubmittersAsync(int projectId);

        public Task<bool> AddProjectManagerAsync(string userId, int projectId);

        public Task RemoveProjectManagerAsync(int projectId);

        public Task<bool> RemoveUserFromProjectAsync(BTUser user, int projectId);

        public Task<bool> IsUserOnProjectAsync(string userId, int projectId);

        public Task<bool> AddUserToProjectAsync(BTUser user, int projectId);
    }
}
