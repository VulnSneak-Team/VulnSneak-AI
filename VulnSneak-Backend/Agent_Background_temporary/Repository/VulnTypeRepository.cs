using Agent_Background_temporary.Models;
using Microsoft.EntityFrameworkCore;

namespace Agent_Background_temporary.Repository
{
    public class VulnTypeRepository
    {
        private AppDbContext _context;
        public VulnTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, VulnType>> GetAllVulnTypeAsync()
        {
            return await _context.VulnTypes.AsNoTracking()
                .ToDictionaryAsync(V => V.VulnName, V => V, StringComparer.OrdinalIgnoreCase);
        }
    }
}
