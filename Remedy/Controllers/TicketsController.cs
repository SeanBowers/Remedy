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
using Remedy.Models.Enums;
using Remedy.Models.ViewModels;
using Remedy.Services.Interfaces;

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

        public TicketsController(ApplicationDbContext context, 
                                UserManager<BTUser> userManager,
                                IBTTicketService ticketService,
                                IBTProjectService projectService,
                                IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _rolesService = rolesService;
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

        // GET: Tickets
        public async Task<IActionResult> UnassignedTickets()
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
                .Where(t => !t.Archived && !t.ArchivedByProject && t.Project!.CompanyId == companyId && t.DeveloperUser == null)
                .ToListAsync();
            return View(ticket);
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
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
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
