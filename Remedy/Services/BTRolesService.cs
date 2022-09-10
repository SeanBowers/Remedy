using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> IsUserInRoleAsync(BTUser member, string roleName)
        {
            try
            {
                return await _userManager.IsInRoleAsync(member, roleName);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            try
            {
                return (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            try
            {
                return (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                return await _context.Roles.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            try
            {
                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roleNames)
        {
            try
            {
                return (await _userManager.RemoveFromRolesAsync(user, roleNames)).Succeeded;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
