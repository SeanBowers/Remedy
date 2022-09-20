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
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTLookupService _lookupService;

        public TicketsController(UserManager<BTUser> userManager,
                                IBTTicketService ticketService,
                                IBTProjectService projectService,
                                IBTRolesService rolesService,
                                IBTFileService fileService,
                                IBTTicketHistoryService ticketHistoryService,
                                IBTNotificationService notificationService,
                                IBTLookupService lookupService)
        {
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _rolesService = rolesService;
            _fileService = fileService;
            _ticketHistoryService = ticketHistoryService;
            _notificationService = notificationService;
            _lookupService = lookupService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var companyId = User.Identity!.GetCompanyId();
            var ticket = await _ticketService.GetTicketsAsync(companyId);
            return View(ticket);
        }

        // GET: My Tickets
        public async Task<IActionResult> MyTickets()
        {
            var companyId = User.Identity!.GetCompanyId();
            var user = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(user, companyId);

            return View(tickets);
        }

        // GET: Unassigned Tickets
        public async Task<IActionResult> UnassignedTickets()
        {
            var companyId = User.Identity!.GetCompanyId();
            var tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);
            return View(tickets);
        }

        // GET: Archived Tickets
        public async Task<IActionResult> ArchivedTickets()
        {
            var companyId = User.Identity!.GetCompanyId();
            var ticket = await _ticketService.GetArchivedTicketsAsync(companyId);

            return View(ticket);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id!.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {

            var companyId = User.Identity!.GetCompanyId();

            ViewData["ProjectId"] = new SelectList(await _projectService.GetProjectsAsync(companyId), "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            var companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            ModelState.Remove("SubmitterUserId");
            if (ModelState.IsValid)
            {
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    ticket.TicketStatusId = (await _lookupService.LookupTicketStatusIdAsync(nameof(BTTicketStatuses.New))).Value;
                    ticket.SubmitterUserId = _userManager.GetUserId(User);
                    ticket.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                    await _ticketService.AddNewTicketAsync(ticket);

                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                    await _ticketHistoryService.AddHistoryAsync(null!, newTicket, userId);


                    BTUser user = await _userManager.GetUserAsync(User);
                    BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId)!;
                    Notification notification = new()
                    {
                        NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationTypes.Ticket))).Value,
                        TicketId = ticket.Id,
                        Title = "New Ticket Added",
                        Message = $"New Ticket: {ticket.Title} was created by {user.FullName}",
                        Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                        SenderId = userId,
                        RecipientId = projectManager?.Id
                    };
                    await _notificationService.AddNotificationAsync(notification);
                    if (projectManager != null)
                    {
                        await _notificationService.SendEmailNotificationAsync(notification, $"New Ticket Added for Porject: {ticket.Project!.Name}");
                    }
                    else
                    {
                        notification.RecipientId = userId;
                        await _notificationService.SendEmailNotificationAsync(notification, $"New Ticket Added for Porject: {ticket.Project!.Name}");
                    }
                }

                return RedirectToAction("Details", "Tickets", new {id = ticket.Id});
            }
            ViewData["ProjectId"] = new SelectList(await _projectService.GetProjectsAsync(companyId), "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id!.Value);

            if (ticket == null)
            {
                return NotFound();
            }
            var projectDevIds = (await _projectService.GetProjectDevelopersAsync(ticket.ProjectId)!);
            var currentDevIds = (await _projectService.GetProjectDevelopersAsync(ticket.ProjectId)!).Select(s => s.Id);

            ViewData["TicketDevelopers"] = new SelectList(projectDevIds, "Id", "FullName", currentDevIds);
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
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
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    int companyId = User.Identity!.GetCompanyId();
                    string userId = _userManager.GetUserId(User);
                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                    try
                    {
                        ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                        ticket.Updated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                        await _ticketService.UpdateTicketAsync(ticket);

                        if (!string.IsNullOrEmpty(DevId))
                        {
                            await _ticketService.AddTicketDeveloperAsync(DevId, ticket.Id);
                        };
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw;
                    }

                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                    await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);
                }
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }

            var projectDevIds = (await _projectService.GetProjectDevelopersAsync(ticket.ProjectId)!);
            var currentDevIds = (await _projectService.GetProjectDevelopersAsync(ticket.ProjectId)!).Select(s => s.Id);

            ViewData["TicketDevelopers"] = new SelectList(projectDevIds, "Id", "FullName", currentDevIds);
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Assign Project Manager
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

        // POST: Assign Developer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicketDeveloper(AssignDeveloperViewModel model)
        {   
            if (!string.IsNullOrEmpty(model.DevID))
            {
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    int companyId = User.Identity!.GetCompanyId();
                    string userId = _userManager.GetUserId(User);

                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id, companyId);
                    try
                    {
                        await _ticketService.AddTicketDeveloperAsync(model.DevID, model.Ticket!.Id);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id, companyId);
                    await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);

                    BTUser user = await _userManager.GetUserAsync(User);
                    Notification notification = new()
                    {
                        NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationTypes.Ticket))).Value,
                        TicketId = model.Ticket.Id,
                        Title = "New Ticket Assignment",
                        Message = $"New Ticket: {model.Ticket.Title}, was assigned by {user.FullName}",
                        Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                        SenderId = userId,
                        RecipientId = model.DevID
                    };
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, "Ticket Assigned");
                }
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

        // POST: Create Comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                    await _ticketService.AddTicketCommentAsync(ticketComment);

                    await _ticketHistoryService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);
                }

            }
            return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
        }

        // POST: Add Attachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                    ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                    ticketAttachment.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                    ticketAttachment.UserId = _userManager.GetUserId(User);

                    await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                    await _ticketHistoryService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);
                }
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        // GET: Show File
        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName)!.Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            
            var ticket = await _ticketService.GetTicketByIdAsync(id!.Value);

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
            
            var ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket != null)
            {
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    await _ticketService.ArchiveTicketAsync(ticket);
                }
            }
            TempData["success"] = "Ticket Archived!";
            return RedirectToAction("Details", "Tickets", new {id=id});
        }

        // GET: Restore
        public async Task<IActionResult> Restore(int? id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id!.Value);

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
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket != null)
            {
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    await _ticketService.RestoreTicketAsync(ticket);
                }
                TempData["success"] = "Ticket Restored!";
                return RedirectToAction("Details", "Tickets", new { id = id });
            }
            TempData["error"] = "Unable to find ticket!";
            return RedirectToAction("Index", "Tickets");

        }
    }
}
