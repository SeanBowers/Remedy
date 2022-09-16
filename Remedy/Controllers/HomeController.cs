using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Remedy.Data;
using Remedy.Extensions;
using Remedy.Models;
using Remedy.Models.Enums;
using Remedy.Models.ViewModels;
using Remedy.Services.Interfaces;
using System.Diagnostics;

namespace Remedy.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTTicketService _ticketService;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              IBTProjectService projectService,
                              UserManager<BTUser> userManager,
                              IBTCompanyService companyService,
                              IBTTicketService ticketService)
        {
            _logger = logger;
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
            _companyService = companyService;
            _ticketService = ticketService;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var companyId = User.Identity!.GetCompanyId();

            DashboardViewModel model = new()
            {
                Projects = await _projectService.GetProjectsAsync(companyId),
                Company = await _companyService.GetCompanyInfoAsync(companyId),
                Members = await _companyService.GetMembersAsync(companyId),
                Tickets = await _ticketService.GetTicketsAsync(companyId)
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectTickets()
        {
            int companyId = User.Identity.GetCompanyId();

            List<Project> projects = await _projectService.GetProjectsAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
            }

            return Json(chartData);
        }
        [HttpPost]
        public async Task<JsonResult> GglProjectPriority()
        {
            int companyId = User.Identity.GetCompanyId();

            List<Project> projects = await _projectService.GetProjectsAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "Priority", "Count" });


            foreach (string priority in Enum.GetNames(typeof(BTProjectPriorities)))
            {
                int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
                chartData.Add(new object[] { priority, priorityCount });
            }

            return Json(chartData);
        }
    }
}
