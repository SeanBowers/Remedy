using Microsoft.EntityFrameworkCore;
using Remedy.Data;
using Remedy.Models;
using Remedy.Services.Interfaces;

namespace Remedy.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites!.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if (invite == null) { return false; }

                try
                {
                    invite.IsValid = false;
                    invite.InviteeId = userId;
                    await _context.SaveChangesAsync();

                    return true;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                await _context.AddAsync(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                return await _context.Invites!.Where(i => i.CompanyId == companyId).AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites!.Where(i => i.CompanyId == companyId && i.Id == inviteId)
                                                .Include(i => i.Company)
                                                .Include(i => i.Project)
                                                .Include(i => i.Invitor)
                                                .FirstOrDefaultAsync();
                return invite!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites!.Where(i => i.CompanyId == companyId && i.CompanyToken == token && i.InviteeEmail == email)
                                                .Include(i => i.Company)
                                                .Include(i => i.Project)
                                                .Include(i => i.Invitor)
                                                .FirstOrDefaultAsync();
                return invite!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            try
            {
                if (token == null) { return false; }

                bool result = false;

                Invite? invite = await _context.Invites!.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if (invite != null)
                {
                    DateTime inviteDate = invite.InviteDate;

                    bool validDate = (DateTime.UtcNow - inviteDate).TotalDays <= 7;

                    if (validDate)
                    {
                        result = invite.IsValid;
                    }
                    else
                    {
                        result = false;
                    }

                }
                return result;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
