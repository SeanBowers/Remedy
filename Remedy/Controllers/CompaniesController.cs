using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Extensions;
using Remedy.Models;
using Remedy.Models.ViewModels;
using Remedy.Services.Interfaces;

namespace Remedy.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _rolesService;

        public CompaniesController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTCompanyService companyService, IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _companyService = companyService;
            _rolesService = rolesService;
        }

        //// GET: Companies
        //public async Task<IActionResult> Index()
        //{
        //    var companyId = (await _userManager.GetUserAsync(User)).CompanyId;
        //    var company = await _context.Companies!.FirstOrDefaultAsync(c => c.Id == companyId);
        //    return View(company);
        //}

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        public async Task<IActionResult> ManageUserRoles()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<ManageUserRolesViewModel> models = new();
            List<BTUser> members = await _companyService.GetMembersAsync(companyId);
            List<IdentityRole> roles = await _rolesService.GetRolesAsync();

            foreach(var member in members)
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
            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(bTUser);
            string? selectedRoles = member.SelectedRoles!.FirstOrDefault();
            if (!string.IsNullOrEmpty(selectedRoles))
            {
                if(await _rolesService.RemoveUserFromRolesAsync(bTUser, currentRoles))
                {
                    await _rolesService.AddUserToRoleAsync(bTUser, selectedRoles);
                }
            }
            return RedirectToAction(nameof(ManageUserRoles));
        }

        //// GET: Companies/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Companies/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageData,ImageType")] Company company)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(company);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(company);
        //}

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageData,ImageType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
