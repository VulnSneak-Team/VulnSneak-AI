using Agent_Background_temporary.Models;
using Microsoft.EntityFrameworkCore;

namespace Agent_Background_temporary.Repository
{
    public class VulnResultRepository
    {
        AppDbContext _context;
        public VulnResultRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VulnResult>> GetAllVulnResultsAsync(int scanSessionId)
        {
            return await _context.VulnResults.Where(VR => VR.ScanId == scanSessionId).ToListAsync();

        }

        public void AddVulnResult(VulnResult result)
        {
            _context.VulnResults.Add(result);
        }
    }
}
