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
    public class NotificationsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTNotificationService _notificationService;

        public NotificationsController(UserManager<BTUser> userManager, IBTNotificationService notificationService)
        {
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            return View(notifications);
        }

    }
}
