using Agent_Background_temporary.Dtos;
using Agent_Background_temporary.Models;
using Agent_Background_temporary.Repository;

namespace Agent_Background_temporary.Services
{
    public class ScanSessionService
    {
        private readonly UnitRepositroy _unitRepositroy;
        public ScanSessionService(UnitRepositroy unitRepositroy)
        {
            _unitRepositroy = unitRepositroy;
        }

        public async Task<List<MessageDto>> GetAllSessionsAsync(int chatId)
        {
            List<MessageDto> messageDtos = new List<MessageDto>();
            List<ScanSession> scanSessions = await _unitRepositroy.sacnSessionRepo.GetAllSessionsAsync(chatId);
            foreach (ScanSession session in scanSessions)
            {
                var messageDto = new MessageDto
                {
                    ContentType = session.ContentType,
                    FileName = session.FileName,
                    File = await TryReadFileAsync(session.FilePath),
                    FileReportName = session.FileReport?.FileReportName,
                    FileReport = await TryReadFileAsync(session.FileReport?.FileReportPath),
                    Status = session.Status ? "Vulnerable" : "Safe",
                    CreatedAt = session.CreatedAt,
                    VulnDtos = session.VulnResults?
                    .Select(v => new VulnDto
                    {
                        Vulnerability_name = v.VulnType!.VulnName,
                        Comment = v.VulnType.Description,
                        Severity = v.VulnType.Severity.ToString(),
                        StartLine = v.StartLine,
                        EndLine = v.EndLine,
                        CodeSnippet = v.CodeSnippet,
                        Confidence = v.Confidence,
                        RepairedCode = v.RepairedCode,
                        Explanation = v.Explanation,
                        ModelUsed = v.ModelUsed,
                        RepairSuccess = v.RepairSuccess,
                        RepairError = v.RepairError,
                        ElapsedSecs = v.ElapsedSecs,
                        VulnLines = v.VulnLines,
                    })
                    .ToList() ?? new List<VulnDto>()
                };
                messageDtos.Add(messageDto);
            }
            return messageDtos;

        }

        public async Task<bool> AnyScanSessionsAsync(int chatId)
        {
            return await _unitRepositroy.sacnSessionRepo.AnyScanSessionsAsync(chatId);
        }
        private static async Task<byte[]?> TryReadFileAsync(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            return await System.IO.File.ReadAllBytesAsync(path);
        }

        public void Remove(ScanSession scanSession)
        {
            _unitRepositroy.sacnSessionRepo.Remove(scanSession);
        }
    }
}
