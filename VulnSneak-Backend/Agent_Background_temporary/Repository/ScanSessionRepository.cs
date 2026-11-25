using Agent_Background_temporary.Models;
using Microsoft.EntityFrameworkCore;

namespace Agent_Background_temporary.Repository
{
    public class ScanSessionRepository
    {
        private AppDbContext _context;

        public ScanSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ScanSession>> GetAllSessionsAsync(int chatId)
        {
            return await _context.ScanSessions
                .AsNoTracking()
                .Where(S => S.ChatId == chatId)
                .Include(S => S.FileReport)
                .Include(S => S.VulnResults)
                    .ThenInclude(VR => VR.VulnType)
                .OrderBy(S => S.CreatedAt)
                .ToListAsync();
        }

        public void AddScanSession(ScanSession scanSession)
        {
            _context.ScanSessions.Add(scanSession);
        }

        public void Remove(ScanSession scanSession)
        {
            _context.ScanSessions.Remove(scanSession);
        }

        public async Task<bool> AnyScanSessionsAsync(int chatId)
        {
            return await _context.ScanSessions.AnyAsync(S => S.ChatId == chatId);
        }
    }
}
