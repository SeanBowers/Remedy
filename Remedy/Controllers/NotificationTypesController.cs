using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Models;

namespace Remedy.Controllers
{
    public class NotificationTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NotificationTypes
        public async Task<IActionResult> Index()
        {
              return _context.NotificationTypes != null ? 
                          View(await _context.NotificationTypes.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.NotificationTypes'  is null.");
        }

        // GET: NotificationTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.NotificationTypes == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notificationType == null)
            {
                return NotFound();
            }

            return View(notificationType);
        }

        // GET: NotificationTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NotificationTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] NotificationType notificationType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notificationType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notificationType);
        }

        // GET: NotificationTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.NotificationTypes == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes.FindAsync(id);
            if (notificationType == null)
            {
                return NotFound();
            }
            return View(notificationType);
        }

        // POST: NotificationTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] NotificationType notificationType)
        {
            if (id != notificationType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notificationType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationTypeExists(notificationType.Id))
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
            return View(notificationType);
        }

        // GET: NotificationTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.NotificationTypes == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notificationType == null)
            {
                return NotFound();
            }

            return View(notificationType);
        }

        // POST: NotificationTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.NotificationTypes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.NotificationTypes'  is null.");
            }
            var notificationType = await _context.NotificationTypes.FindAsync(id);
            if (notificationType != null)
            {
                _context.NotificationTypes.Remove(notificationType);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationTypeExists(int id)
        {
          return (_context.NotificationTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
