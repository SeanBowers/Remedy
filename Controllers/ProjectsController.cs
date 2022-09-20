using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Extensions;
using Remedy.Models;
using Remedy.Models.Enums;
using Remedy.Models.ViewModels;
using Remedy.Services.Interfaces;

namespace Remedy.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTLookupService _lookupService;

        public ProjectsController(UserManager<BTUser> userManager,
                                  IBTFileService fileService,
                                  IBTProjectService projectService,
                                  IBTRolesService rolesService,
                                  IBTLookupService lookupService)
        {
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _rolesService = rolesService;
            _lookupService = lookupService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            var projects = await _projectService.GetProjectsAsync(companyId);
            return View(projects);
        }

        public async Task<IActionResult> MyProjects()
        {
            var userId = _userManager.GetUserId(User);
            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        [Authorize(Roles = "Admin")]
        // GET: AssignProjectManager
        public async Task<IActionResult> AssignProjectManager(int? id)
        {
            if (id == null) { return NotFound(); }

            AssignPMViewModel model = new();

            int companyId = User.Identity!.GetCompanyId();

            model.Project = await _projectService.GetProjectByIdAsync(id.Value);

            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMID))
            {
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    await _projectService.AddProjectManagerAsync(model.PMID, model.Project!.Id);
                }
                TempData["success"] = "Project Manager Assigned!";
                return RedirectToAction(nameof(Index));
            };

            int companyId = User.Identity!.GetCompanyId();

            model.Project = await _projectService.GetProjectByIdAsync(model.Project!.Id);

            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);

            TempData["error"] = "No Project Manager Chosen! Please select a PM.";
            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: AssignProjectMembers
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if (id == null) { return NotFound(); }

            AssignMembersViewModel model = new();

            int companyId = User.Identity!.GetCompanyId();

            model.Project = await _projectService.GetProjectByIdAsync(id.Value);

            var currentDevIds = (await _projectService.GetProjectDevelopersAsync(model.Project.Id)!).Select(s => s.Id);
            var currentSubIds = (await _projectService.GetProjectSubmittersAsync(model.Project.Id)!).Select(s => s.Id);

            model.DevList = new MultiSelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), "Id", "FullName", currentDevIds);
            model.SubList = new MultiSelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId), "Id", "FullName", currentSubIds);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // GET: AssignProjectMembers
        public async Task<IActionResult> AssignProjectMembers(AssignMembersViewModel model, List<string> SubID, List<string> DevID)
        {
            if (model.SubID != null || model.DevID != null)
            {
                int companyId = User.Identity!.GetCompanyId();
                var devs = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
                var subs = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
                devs.AddRange(subs);
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    foreach (var user in devs)
                    {
                        await _projectService.RemoveUserFromProjectAsync(user, model.Project!.Id);
                    }

                    SubID.AddRange(DevID);
                    foreach (var userId in SubID)
                    {
                        await _projectService.AddUserToProjectAsync(userId!, model.Project!.Id);
                    }
                }
                TempData["success"] = "Members Assigned!";
                return RedirectToAction("Details", "Projects", new { id = model.Project!.Id });
            };

            return View(model);
        }

        // GET: Archived Projects
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> ArchivedProjects()
        {
            var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            var projects = await _projectService.GetArchivedProjectsAsync(companyId);
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) { return NotFound(); }

            var project = await _projectService.GetProjectByIdAsync(id.Value);

            if (project == null) { return NotFound(); }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            // TODO: Abstract the use of _context
            ViewData["ProjectPriorityId"] = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");
            ViewData["ProjectManager"] = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId,ProjectImg, Members")] Project project, string? PMID)
        {
            if (ModelState.IsValid)
            {
                project.CompanyId = User.Identity!.GetCompanyId();
                project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                project.StartDate = DateTime.SpecifyKind(project.StartDate!.Value, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate!.Value, DateTimeKind.Utc);
                if (project.ProjectImg != null)
                {
                    project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ProjectImg);
                    project.ImageType = project.ProjectImg.ContentType;
                }
                TempData["success"] = "Project Created Successfully!";
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    await _projectService.AddProjectAsync(project);
                }
                if (!string.IsNullOrEmpty(PMID))
                {
                    if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                    {
                        await _projectService.AddProjectManagerAsync(PMID, project!.Id);
                    }
                };
                return RedirectToAction("Details", "Projects", new { id = project.Id });
            }
            ViewData["ProjectPriorityId"] = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) { return NotFound(); }

            var project = await _projectService.GetProjectByIdAsync(id.Value);

            if (project == null)
            {
                return NotFound();
            }
            var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            string? currentPMId = (await _projectService.GetProjectManagerAsync(project.Id)!)?.Id;
            ViewData["ProjectPriorityId"] = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name", project.ProjectPriorityId);
            ViewData["ProjectManager"] = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageData,ImageType,ProjectImg")] Project project, string? PMID)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.Created = DateTime.SpecifyKind(project.Created, DateTimeKind.Utc);
                    project.StartDate = DateTime.SpecifyKind(project.StartDate!.Value, DateTimeKind.Utc);
                    project.EndDate = DateTime.SpecifyKind(project.EndDate!.Value, DateTimeKind.Utc);
                    if (project.ProjectImg != null)
                    {
                        project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ProjectImg);
                        project.ImageType = project.ProjectImg.ContentType;
                    }
                    if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                    {
                        await _projectService.UpdateProjectAsync(project);
                    }
                    if (!string.IsNullOrEmpty(PMID))
                    {
                        if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                        {
                            await _projectService.AddProjectManagerAsync(PMID, project!.Id);
                        }
                    };
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw;
                }
                TempData["success"] = "Project Edited Successfully!";
                return RedirectToAction("Details", "Projects", new { id = id });
            }
            ViewData["ProjectPriorityId"] = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            var project = await _projectService.GetProjectByIdAsync(id!.Value);
            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (!User.IsInRole(nameof(BTRoles.DemoUser)))
            {
                await _projectService.ArchiveProjectAsync(id);
            }
            TempData["success"] = "Project Archived!";
            return RedirectToAction("Details", "Projects", new { id = id });
        }

        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            var project = await _projectService.GetProjectByIdAsync(id!.Value);
            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreProject(int id)
        {
            if (!User.IsInRole(nameof(BTRoles.DemoUser)))
            {
                await _projectService.RestoreProjectAsync(id);
            }
            TempData["success"] = "Project Restored!";
            return RedirectToAction("Details", "Projects", new { id = id });
        }

    }
}