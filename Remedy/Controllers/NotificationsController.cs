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

namespace Remedy.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var notifications = await _context.Notifications!
                .Include(n => n.NotificationType)
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Where(n => n.RecipientId == userId || n.SenderId == userId)
                .ToListAsync();

            return View(notifications);
        }

        // GET: Notifications/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.Notifications == null)
        //    {
        //        return NotFound();
        //    }

        //    var notification = await _context.Notifications
        //        .Include(n => n.NotificationType)
        //        .Include(n => n.Recipient)
        //        .Include(n => n.Sender)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (notification == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(notification);
        //}

        // GET: Notifications/Create
        public IActionResult Create()
        {
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Id");
            ViewData["RecipientId"] = new SelectList(_context.Set<BTUser>(), "Id", "Id");
            ViewData["SenderId"] = new SelectList(_context.Set<BTUser>(), "Id", "Id");
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,TicketId,Title,Message,Created,SenderId,RecipientId,NotificationTypeId,HasBeenViewed")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Id", notification.NotificationTypeId);
            ViewData["RecipientId"] = new SelectList(_context.Set<BTUser>(), "Id", "Id", notification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Set<BTUser>(), "Id", "Id", notification.SenderId);
            return View(notification);
        }

        // GET: Notifications/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Notifications == null)
        //    {
        //        return NotFound();
        //    }

        //    var notification = await _context.Notifications.FindAsync(id);
        //    if (notification == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Id", notification.NotificationTypeId);
        //    ViewData["RecipientId"] = new SelectList(_context.Set<BTUser>(), "Id", "Id", notification.RecipientId);
        //    ViewData["SenderId"] = new SelectList(_context.Set<BTUser>(), "Id", "Id", notification.SenderId);
        //    return View(notification);
        //}

        //// POST: Notifications/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,TicketId,Title,Message,Created,SenderId,RecipientId,NotificationTypeId,HasBeenViewed")] Notification notification)
        //{
        //    if (id != notification.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(notification);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!NotificationExists(notification.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Id", notification.NotificationTypeId);
        //    ViewData["RecipientId"] = new SelectList(_context.Set<BTUser>(), "Id", "Id", notification.RecipientId);
        //    ViewData["SenderId"] = new SelectList(_context.Set<BTUser>(), "Id", "Id", notification.SenderId);
        //    return View(notification);
        //}

        //// GET: Notifications/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Notifications == null)
        //    {
        //        return NotFound();
        //    }

        //    var notification = await _context.Notifications
        //        .Include(n => n.NotificationType)
        //        .Include(n => n.Recipient)
        //        .Include(n => n.Sender)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (notification == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(notification);
        //}

        //// POST: Notifications/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Notifications == null)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.Notifications'  is null.");
        //    }
        //    var notification = await _context.Notifications.FindAsync(id);
        //    if (notification != null)
        //    {
        //        _context.Notifications.Remove(notification);
        //    }
            
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool NotificationExists(int id)
        {
          return (_context.Notifications?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
