using Agent_Background_temporary.Models;
using Microsoft.EntityFrameworkCore;

namespace Agent_Background_temporary.Repository
{
    public class FileReportRepository
    {
        private AppDbContext _context;
        public FileReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FileReport?> GetFileReportAsync(int scanSessionId)
        {
            return await _context.FileReports.FirstOrDefaultAsync(FR => FR.ScanId == scanSessionId);
        }

        public void AddFileReport(FileReport fileReport)
        {
            _context.FileReports.Add(fileReport);
        }
    }
}
