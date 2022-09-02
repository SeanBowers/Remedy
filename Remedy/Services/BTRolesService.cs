using Microsoft.AspNetCore.Identity;
using Remedy.Data;
using Remedy.Models;
using Remedy.Services.Interfaces;

namespace Remedy.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public BTRolesService(RoleManager<IdentityRole> roleManager,
                              ApplicationDbContext context,
                              UserManager<BTUser> userManager)
        {
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            try
            {
                return (await _userManager.GetUsersInRoleAsync(roleName)).Where(b => b.CompanyId == companyId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
