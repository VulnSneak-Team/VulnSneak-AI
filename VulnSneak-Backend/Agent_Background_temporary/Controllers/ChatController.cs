using Agent_Background_temporary.Dtos;
using Agent_Background_temporary.Models;
using Agent_Background_temporary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agent_Background_temporary.Controllers
{
    [Route("api/v1/[controller]")]  // api/v1/Chat
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
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
        public ChatController(UnitServices unitServices)
        {
            _unitServices = unitServices;
        }

        [HttpGet("GetAllChats")]  // GET  api/v1/Chat/GetAllChats
        public async Task<IActionResult> GetAllChatsAsync()
        {
            User? user = await _unitServices.userService.GetUserAsync(UserId);
            if (user == null)
                return Unauthorized();

            List<ChatDto> chatDtos = await _unitServices.chatService.GetAllChatsAsync(UserId);

            return Ok(chatDtos);
        }


        [HttpPost]  // POST  api/v1/Chat  
        public async Task<IActionResult> AddChatAsync(ChatDto chatDto)
        {
            if (string.IsNullOrWhiteSpace(chatDto?.ChatName))
                return BadRequest(new { message = "Chat name is required." });

            User? user = await _unitServices.userService.GetUserAsync(UserId);

            if (user is null)
                return Unauthorized();

            Chat? chat = await _unitServices.chatService.AddChatAsync(user, chatDto);
            await _unitServices.SaveInDataBaseAsync();

            return Ok(new ChatDto
            {
                ChatId = chat.Id,
                ChatName = chat.ChatName,
                CreatedAt = chat.CreatedAt
            });
        }


        [HttpGet("{chatId:int}")]  // GET  api/v1/Chat/{chatId:int}
        public async Task<IActionResult> GetChatAsync(int chatId)
        {
            if (chatId <= 0)
                return BadRequest(new { message = "Invalid chat ID." });

            User? user = await _unitServices.userService.GetUserAsync(UserId);
            Chat? chat = await _unitServices.chatService.GetChatAsync(UserId, chatId);

            if (user == null)
                return Unauthorized();

            if (chat == null)
                return NotFound();

            List<MessageDto> scanSessions = await _unitServices.scanSessionService.GetAllSessionsAsync(chatId);

            if (scanSessions == null)
                return Ok(Enumerable.Empty<MessageDto>());

            return Ok(scanSessions);
        }

        [HttpPut("Rename")]  //  PUT  api/v1/Chat/Rename
        public async Task<IActionResult> RenameChatAsync(ChatDto chatDto)
        {
            if (string.IsNullOrWhiteSpace(chatDto.ChatName) || chatDto.ChatId <= 0)
                return BadRequest(new { message = "Try Again" });

            User? user = await _unitServices.userService.GetUserAsync(UserId);
            Chat? chat = await _unitServices.chatService.GetChatAsync(UserId, chatDto.ChatId, Tracking: true);

            if (user == null)
                return Unauthorized();

            if (chat == null)
                return NotFound();

            bool chatNameUpdated = await _unitServices.chatService.UpdateChatName(chat, chatDto);
            if (!chatNameUpdated)
                return StatusCode(500);

            await _unitServices.SaveInDataBaseAsync();
            return Ok();
        }


        [HttpDelete("{chatId:int}")]  // DELETE  api/v1/Chat/{ChatId:int}
        public async Task<IActionResult> DeleteChatAsync(int chatId)
        {
            if (chatId <= 0)
                return NotFound();

            User? user = await _unitServices.userService.GetUserAsync(UserId);
            if (user == null)
                return Unauthorized();

            Chat? chat = await _unitServices.chatService.GetChatAsync(UserId, chatId);
            if (chat is null)
                return NotFound();
            try
            {
                await _unitServices.chatService.DeleteChatAsync(chat);
                await _unitServices.SaveInDataBaseAsync();
                if (!await _unitServices.chatService.AnyChatsAsync(UserId))
                {
                    Directory.Delete(Directory.GetParent(chat.ChatFolderPath)!.FullName, recursive: true);
                }
                return Ok(new { message = "Chat deleted successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An internal error occurred." });

            }

        }
    }
}