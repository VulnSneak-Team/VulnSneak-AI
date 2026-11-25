using Agent_Background_temporary.Dtos;
using Agent_Background_temporary.Models;
using Agent_Background_temporary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Agent_Background_temporary.Controllers
{
    [Route("api/v1/[controller]")] //  api/v1/message
    [ApiController]
    [Authorize]

    public class MessageController : ControllerBase
    {
        private readonly UnitServices _unitServices;

        private int UserId
        {
            get
            {
                if (field == default)
                    int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value, out field);
                return field;
            }
        }

        public MessageController(UnitServices unitServices)
        {
            _unitServices = unitServices;
        }

        [HttpPost("{chatId:int}")]
        public async Task<IActionResult> AddMessageAsync(
            IFormFile? formFile,
            string? message,
            IHttpClientFactory httpClientFactory,
            int chatId)
        {
            // ─── Validate request ────────────────────────────────────────────────────
            if ((chatId <= 0 || formFile == null || formFile.Length == 0) && string.IsNullOrWhiteSpace(message))
                return BadRequest(new { message = "Invalid request." });

            // ─── Auth check ──────────────────────────────────────────────────────────
            Chat? chat = await _unitServices.chatService.GetChatAsync(UserId, chatId);
            if (chat == null)
                return Unauthorized();

            if (!string.IsNullOrWhiteSpace(message))
            {
                var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(120);

                //var scanSessions = await Context.ScanSessions
                //.AsNoTracking()
                //.Where(s => s.ChatId == chatId)
                //.Include(s => s.VulnResults)
                //    .ThenInclude(v => v.VulnType)
                //.OrderBy(s => s.CreatedAt)
                //.ToListAsync();

                //if (!scanSessions.Any())
                //    return Ok(Enumerable.Empty<MessageDto>());

                //var messageDtos = await Task.WhenAll(scanSessions.Select(ChatController.MapToMessageDtoAsync));
                //var str = string.Join("\n", messageDtos.Select(m => m.ToString()));
                var payload = JsonSerializer.Serialize(new
                {
                    message,
                    history = new[] { new { role = "user", content = "" } }
                });
                //return Ok(payload);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:8000/chat", content);
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode);

                var result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }
            var scanSession = new ScanSession { Status = default };
            try
            {
                // ─── Validate & Save file ──────────────────────────────────────────────────
                scanSession = await _unitServices.SaveFileAsync(formFile, chat);

                // ─── Send file to AI Model ────────────────────────────────────────────────-
                string json = await _unitServices.AiScanFileAsync(scanSession.FilePath, formFile);

                // ─── save response details ─────────────────────────────────────────────────
                ScanSessionDto scanSessionDto = await _unitServices.ParseAiResponseAsync(json, scanSession, chat);

                await _unitServices.SaveInDataBaseAsync();

                return Ok(scanSessionDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mesage = ex.Message });
            }
            catch (Exception)
            {
                if (scanSession != null)
                {
                    _unitServices.scanSessionService.Remove(scanSession);
                    await _unitServices.SaveInDataBaseAsync();

                    if (System.IO.File.Exists(scanSession.FilePath))
                    {
                        if (!await _unitServices.scanSessionService.AnyScanSessionsAsync(chatId))
                        {
                            await _unitServices.chatService.DeleteChatAsync(chat);
                            await _unitServices.SaveInDataBaseAsync();
                            if (!await _unitServices.chatService.AnyChatsAsync(UserId))
                            {
                                Directory.Delete(Directory.GetParent(chat.ChatFolderPath)!.FullName, recursive: true);
                            }
                        }
                        else
                            System.IO.File.Delete(scanSession.FilePath);
                    }
                }
                return StatusCode(500, new { message = "An internal error occurred." });
            }
        }
    }
}
