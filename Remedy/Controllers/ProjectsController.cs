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
using Remedy.Models;
using Remedy.Services.Interfaces;

namespace Remedy.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IBTProjectService _projectService;

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IFileService fileService,
                                  IBTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;   
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            var projects = await _projectService.GetProjectsAsync(companyId);
            return View(projects);
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
            if (id == null){return NotFound();}

            var project = await _projectService.GetProjectByIdAsync((int)id);

            if (project == null){return NotFound();}

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public IActionResult Create()
        {
            // TODO: Abstract the use of _context
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId,ProjectImg")] Project project)
        {
            if (ModelState.IsValid)
            {
                // TODO: Make CompanyId retrieval more efficient.
                project.CompanyId = (await _userManager.GetUserAsync(User)).CompanyId;
                project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                project.StartDate = DateTime.SpecifyKind(project.StartDate!.Value, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate!.Value, DateTimeKind.Utc);
                if (project.ProjectImg != null)
                {
                    project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ProjectImg);
                    project.ImageType = project.ProjectImg.ContentType;
                }
                TempData["success"] = "Project Created Successfully!";
                await _projectService.AddProjectAsync(project);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null){return NotFound();}

            var project = await _projectService.GetProjectByIdAsync((int)id);

            if (project == null){return NotFound();
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageData,ImageType,ProjectImg")] Project project)
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
                    await _projectService.UpdateProjectAsync(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Project Edited Successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            var project = await _projectService.GetProjectByIdAsync((int)id);
            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            await _projectService.ArchiveProjectAsync(id);
            TempData["success"] = "Project Archived!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            var project = await _projectService.GetProjectByIdAsync((int)id);
            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreProject(int id)
        {
            await _projectService.RestoreProjectAsync(id);
            TempData["success"] = "Project Restored!";
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}