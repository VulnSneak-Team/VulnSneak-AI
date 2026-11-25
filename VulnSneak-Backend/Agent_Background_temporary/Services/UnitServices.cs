using Agent_Background_temporary.Dtos;
using Agent_Background_temporary.Models;
using Agent_Background_temporary.Repository;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Agent_Background_temporary.Services
{
    public class UnitServices
    {
        private readonly UnitRepositroy _unitRepo;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public UnitServices(UnitRepositroy unitRepo, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _unitRepo = unitRepo;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public ChatService chatService
        {
            get
            {
                if (field == null)
                    return new ChatService(_unitRepo, _configuration);
                return field;
            }
        }

        public UserService userService
        {
            get
            {
                if (field == null)
                    return new UserService(_unitRepo, _configuration);
                return field;
            }
        }

        public ScanSessionService scanSessionService
        {
            get
            {
                if (field == null)
                    return new ScanSessionService(_unitRepo);
                return field;
            }
        }

        public async Task<ScanSession> SaveFileAsync(IFormFile? formFile, Chat chat)
        {
            // ─── Validate file ──────────────────────────────────────────────────
            ValidateFile(formFile);

            string originFolder = Path.Combine(chat.ChatFolderPath, "Origin");
            Directory.CreateDirectory(originFolder);

            var scanSession = new ScanSession
            {
                FileName = Path.GetFileName(formFile!.FileName),
                ContentType = formFile.ContentType,
                ChatId = chat.Id,
                Status = false
            };
            _unitRepo.sacnSessionRepo.AddScanSession(scanSession);
            await SaveInDataBaseAsync();
            string filePath = Path.Combine(
                originFolder,
                $"{scanSession.Id}_{SanitizeForPath(scanSession.FileName)}"
            );
            scanSession.FilePath = filePath;
            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await formFile.CopyToAsync(fs);

            return scanSession;
        }

        private void ValidateFile(IFormFile? formFile)
        {
            int maxFileSizeMegaBytes = int.Parse(_configuration["FileUploadOptions:MaxFileSizeMegaBytes"]!);
            if ((formFile!.Length / (1024 * 1024)) > maxFileSizeMegaBytes)
                throw new ArgumentException(
                    $"File exceeds the maximum size of {maxFileSizeMegaBytes}MB.");

            HashSet<string> allowedExtentions = _configuration.GetSection("FileUploadOptions:AllowedExtentions")
                .Get<string[]>()!.ToHashSet(StringComparer.OrdinalIgnoreCase)!;
            string ext = Path.GetExtension(formFile.FileName);
            if (!allowedExtentions.Contains(ext))
                throw new ArgumentException($"File extension '{ext}' is not alllowed.");
        }

        public async Task<string> AiScanFileAsync(string filePath, IFormFile? formFile)
        {
            string agentUrl = _configuration["AIModel:Url"]!;

            using var multipart = new MultipartFormDataContent();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(formFile!.ContentType);

            multipart.Add(fileContent, "file", formFile!.FileName);

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(120);
            var response = await client.PostAsync(agentUrl, multipart);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("AI Model returned an empty response.");

            return json;
        }

        public async Task<ScanSessionDto> ParseAiResponseAsync(string json, ScanSession scanSession, Chat chat)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Dictionary<string, JsonElement> jsonData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, jsonOptions)
                       ?? throw new Exception("Failed to desrialize AI response.");

            string? statusStr =
                jsonData.TryGetValue("Status", out var statusEl) ? statusEl.GetString() : null;

            if (!Enum.TryParse<ScanStatus>(statusStr, out var status))
                throw new Exception($"Unknown AI status: '{statusStr}'.");

            // ─── FastAPI HTTPException errors ────────────────────────────────────
            if (jsonData.TryGetValue("detail", out var detailEl))
                throw new Exception(detailEl.GetString());

            // ─── Vulnerable ──────────────────────────────────────────────────────
            if (status == ScanStatus.Vulnerable)
            {
                scanSession.Status = true;
                var scanSessionDto = new ScanSessionDto
                {
                    Status = status.ToString(),
                    CreatedAt = scanSession.CreatedAt
                };

                // ─── Save repaired file to disk ───────────────────────────────────
                if (jsonData.TryGetValue("RepairedFile", out var repairedFileEl) && jsonData.TryGetValue("RepairedFilename", out var repairedFileNameEl))
                    await SaveRepairFileAsync(scanSession, chat, scanSessionDto, repairedFileEl, repairedFileNameEl);

                // ─── Parse Findings (PascalCase) ─────────────────────────────────
                if (jsonData.TryGetValue("Findings", out var findingsEl)
                    && findingsEl.ValueKind == JsonValueKind.Array)
                {
                    var vulnTypes = await _unitRepo.VulnTypeRepo.GetAllVulnTypeAsync();

                    foreach (var finding in findingsEl.EnumerateArray())
                        MapTo(finding, vulnTypes, scanSession, scanSessionDto);
                }
                return scanSessionDto;
            }
            return new ScanSessionDto { Status = "Safe", CreatedAt = scanSession.CreatedAt };
        }

        private async Task SaveRepairFileAsync(ScanSession scanSession, Chat chat, ScanSessionDto scanSessionDto, JsonElement repairedFileEl, JsonElement repairedFileNameEl)
        {
            string? repairedContent = repairedFileEl.GetString();
            string? repairedFilename = repairedFileNameEl.GetString();

            if (!string.IsNullOrEmpty(repairedContent) && !string.IsNullOrEmpty(repairedFilename))
            {
                string repairedFolder = Path.Combine(chat.ChatFolderPath, "Repaired");
                Directory.CreateDirectory(repairedFolder);

                string repairedFilePath = Path.Combine(
                    repairedFolder,
                    $"{scanSession.Id}_{SanitizeForPath(repairedFilename)}"
                );

                await File.WriteAllTextAsync(repairedFilePath, repairedContent);

                _unitRepo.fileReportRepo.AddFileReport(new FileReport()
                {
                    ScanId = scanSession.Id,
                    FileReportName = repairedFilename,
                    FileReportPath = repairedFilePath
                });

                scanSessionDto.RepairedFileName = repairedFilename;
                scanSessionDto.RepairedFile = await TryReadFileAsync(repairedFilePath);
            }
        }

        private void MapTo(JsonElement finding, Dictionary<string, VulnType> vulnTypes, ScanSession scanSession, ScanSessionDto scanSessionDto)
        {
            string? vulnName = finding.GetProperty("VulnName").GetString() ?? "";

            if (!vulnTypes.TryGetValue(vulnName, out var vulnType))
                return;

            var vulnResult = new VulnResult()
            {
                ScanId = scanSession.Id,
                VulnTypeId = vulnType.Id
            };
            var vulnDto = new VulnDto
            {
                Vulnerability_name = vulnName,
                Comment = vulnType.Description,
                Severity = vulnType.Severity.ToString()
            };

            vulnResult.StartLine = vulnDto.StartLine = finding.GetProperty("StartLine").GetInt32();
            vulnResult.EndLine = vulnDto.EndLine = finding.GetProperty("EndLine").GetInt32();
            vulnResult.CodeSnippet = vulnDto.CodeSnippet = finding.GetProperty("CodeSnippet").GetString();
            vulnResult.Confidence = vulnDto.Confidence = finding.GetProperty("Confidence").GetDouble();

            if (finding.TryGetProperty("Repair", out var repairEl))
            {
                vulnResult.RepairedCode = vulnDto.RepairedCode = repairEl.GetProperty("RepairedCode").GetString();
                vulnResult.Explanation = vulnDto.Explanation = repairEl.GetProperty("Explanation").GetString();
                vulnResult.ModelUsed = vulnDto.ModelUsed = repairEl.GetProperty("ModelUsed").GetString();
                vulnResult.RepairSuccess = vulnDto.RepairSuccess = repairEl.GetProperty("Success").GetBoolean();
                vulnResult.RepairError = vulnDto.RepairError = repairEl.GetProperty("Error").GetString();
                vulnResult.ElapsedSecs = vulnDto.ElapsedSecs = repairEl.GetProperty("ElapsedSecs").GetDouble();
            }

            var vulnLines = new List<int>();
            if (finding.TryGetProperty("VulnLines", out var vulnLinesEl)
                && vulnLinesEl.ValueKind == JsonValueKind.Array)
            {
                vulnLines = vulnLinesEl.EnumerateArray()
                    .Select(x => x.GetInt32())
                    .ToList();
            }
            vulnResult.VulnLines = vulnDto.VulnLines = vulnLines;

            scanSessionDto.VulnDtos.Add(vulnDto);
            _unitRepo.vulnResultRepo.AddVulnResult(vulnResult);
        }

        public async Task SaveInDataBaseAsync()
        {
            await _unitRepo.SaveInDataBaseAsync();
        }

        public static string SanitizeForPath(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return String.Empty;
            var invalid = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct().ToArray();
            var cleaned = new string(s.Where(ch => !invalid.Contains(ch)).ToArray());
            return cleaned.Replace(' ', '_');
        }

        private static async Task<byte[]?> TryReadFileAsync(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            return await File.ReadAllBytesAsync(path);
        }
    }
}
