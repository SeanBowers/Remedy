using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Models;
using Remedy.Models.Enums;
using Remedy.Models.ViewModels;
using Remedy.Services.Interfaces;
using Remedy.Extensions;

namespace Remedy.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;

        public TicketsController(ApplicationDbContext context,
                                UserManager<BTUser> userManager,
                                IBTTicketService ticketService,
                                IBTProjectService projectService,
                                IBTRolesService rolesService,
                                IBTFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _rolesService = rolesService;
            _fileService = fileService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            var ticket = await _context.Tickets!
                .Include(t => t.Project)
                .ThenInclude(t => t.Company)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Where(t => !t.Archived && !t.ArchivedByProject && t.Project!.CompanyId == companyId)
                .OrderByDescending(t => t.ProjectId)
                .ThenByDescending(t => t.TicketPriority)
                .ToListAsync();
            return View(ticket);
        }

        public async Task<IActionResult> MyTickets()
        {
            var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            var user = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(user, companyId);

            return View(tickets);
        }

        // GET: Tickets
        public async Task<IActionResult> UnassignedTickets()
        {
            var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            var tickets = await _context.Tickets!
                .Include(t => t.Project)
                .ThenInclude(t => t.Company)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Where(t => !t.Archived && !t.ArchivedByProject && t.Project!.CompanyId == companyId && t.DeveloperUser == null)
                .ToListAsync();
            return View(tickets);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: AssignProjectManager
        public async Task<IActionResult> AssignTicketDeveloper(int? id)
        {
            if (id == null) { return NotFound(); }

            AssignDeveloperViewModel model = new();

            model.Ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            var projectDevIds = (await _projectService.GetProjectDevelopersAsync(model.Ticket.ProjectId)!);
            var currentDevIds = (await _projectService.GetProjectDevelopersAsync(model.Ticket.ProjectId)!).Select(s => s.Id);

            model.DevList = new SelectList(projectDevIds, "Id", "FullName", currentDevIds);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicketDeveloper(AssignDeveloperViewModel model)
        {
            if (!string.IsNullOrEmpty(model.DevID))
            {
                await _ticketService.AddTicketDeveloperAsync(model.DevID, model.Ticket!.Id);

                TempData["success"] = "Ticket Developer Assigned!";
                return RedirectToAction(nameof(Index));
            };

            model.Ticket = await _ticketService.GetTicketByIdAsync(model.Ticket!.Id);

            var projectDevIds = (await _projectService.GetProjectDevelopersAsync(model.Ticket.ProjectId)!);
            var currentDevIds = (await _projectService.GetProjectDevelopersAsync(model.Ticket.ProjectId)!).Select(s => s.Id);

            model.DevList = new SelectList(projectDevIds, "Id", "FullName", currentDevIds);

            TempData["error"] = "No Developer Chosen! Please select a Developer.";
            return View(model);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.TicketAttachments)
                .Include(t => t.TicketComments!).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                ticketComment.UserId = _userManager.GetUserId(User);
                ticketComment.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                _context.Add(ticketComment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            return View(Details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName)!.Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {

            var companyId = User.Identity!.GetCompanyId();

            ViewData["ProjectId"] = new SelectList(await _projectService.GetProjectsAsync(companyId), "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            var companyId = User.Identity!.GetCompanyId();
            ModelState.Remove("SubmitterUserId");
            if (ModelState.IsValid)
            {
                ticket.TicketStatusId = (await _context.TicketStatuses!.FirstOrDefaultAsync(s => s.Name == nameof(BTTicketStatuses.New)))!.Id;
                ticket.SubmitterUserId = _userManager.GetUserId(User);
                ticket.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(await _projectService.GetProjectsAsync(companyId), "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            var projectDevIds = (await _projectService.GetProjectDevelopersAsync(ticket.ProjectId)!);
            var currentDevIds = (await _projectService.GetProjectDevelopersAsync(ticket.ProjectId)!).Select(s => s.Id);

            ViewData["TicketDevelopers"] = new SelectList(projectDevIds, "Id", "FullName", currentDevIds);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket, string? DevId)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    if (!string.IsNullOrEmpty(DevId))
                    {
                        await _ticketService.AddTicketDeveloperAsync(DevId, ticket.Id);
                    };
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.Archived = true;
            }
            TempData["success"] = "Ticket Archived!";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ArchivedTickets()
        {
            var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            var ticket = await _context.Tickets!
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .ThenInclude(t => t.Company)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Where(t => t.Archived || t.ArchivedByProject && t.Project!.CompanyId == companyId)
                .ToListAsync();
            return View(ticket);
        }
        // GET: Restore
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (ticket.ArchivedByProject)
            {
            TempData["error"] = "Ticket is archived by project! Restore project to make ticket active!";

            }
            return View(ticket);
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreTicket(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.Archived = false;
                await _context.SaveChangesAsync();
                TempData["success"] = "Ticket Restored!";
                return RedirectToAction("Details", "Tickets", new { id = id });
            }
            TempData["error"] = "Unable to find ticket!";
            return RedirectToAction("Index", "Tickets");

        }
        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
