using Remedy.Models;

namespace Remedy.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<List<BTUser>> GetMembersAsync(int? companyId);

        public Task<Company> GetCompanyInfoAsync(int? companyId);

        public Task<bool> UpdateCompanyAsync(Company company);
    }
}
