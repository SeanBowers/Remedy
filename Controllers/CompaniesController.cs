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
    [Authorize(Roles = "Admin")]
    public class CompaniesController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _rolesService;

        public CompaniesController(UserManager<BTUser> userManager, IBTCompanyService companyService, IBTRolesService rolesService)
        {
            _userManager = userManager;
            _companyService = companyService;
            _rolesService = rolesService;
        }

        public async Task<IActionResult> ManageUserRoles()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<ManageUserRolesViewModel> models = new();
            List<BTUser> members = await _companyService.GetMembersAsync(companyId);
            List<IdentityRole> roles = await _rolesService.GetRolesAsync();

            foreach (var member in members)
            {
                models.Add(new ManageUserRolesViewModel()
                {
                    BTUser = member,
                    Roles = new MultiSelectList(roles, "Name", "Name", await _rolesService.GetUserRolesAsync(member))
                });

            }

            return View(models);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            int companyId = User.Identity!.GetCompanyId();
            BTUser? bTUser = (await _companyService.GetMembersAsync(companyId)).FirstOrDefault(m => m.Id == member.BTUser!.Id);
            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(bTUser!);
            string? selectedRoles = member.SelectedRoles!.FirstOrDefault();
            if (!string.IsNullOrEmpty(selectedRoles))
            {
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    if (await _rolesService.RemoveUserFromRolesAsync(bTUser!, currentRoles))
                    {
                        await _rolesService.AddUserToRoleAsync(bTUser!, selectedRoles);
                    }
                }
            }
            return RedirectToAction(nameof(ManageUserRoles));
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int companyId = User.Identity!.GetCompanyId();
            var company = await _companyService.GetCompanyInfoAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!User.IsInRole(nameof(BTRoles.DemoUser))){
                        await _companyService.UpdateCompanyAsync(company);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction("Dashboard", "Home");
            }
            return View(company);
        }
    }
}
