using Agent_Background_temporary.Dtos;
using Agent_Background_temporary.Models;
using Agent_Background_temporary.Repository;
namespace Agent_Background_temporary.Services
{
    public class ChatService
    {
        private UnitRepositroy _unitRepo;
        private string? _basePath;

        public ChatService(UnitRepositroy unitOfWork, IConfiguration configuration)
        {
            _unitRepo = unitOfWork;
            _basePath = configuration["FileStorage:BasePath"];
        }

        public async Task<List<ChatDto>> GetAllChatsAsync(int UserId)
        {
            List<Chat> chats = await _unitRepo.chatRepo.GetAllChatsAsync(UserId);
            return chats.Select(chat => new ChatDto
            {
                ChatId = chat.Id,
                ChatName = chat.ChatName,
                CreatedAt = chat.CreatedAt
            }).ToList();
        }

        public async Task<Chat?> GetChatAsync(int userId, int chatId, bool Tracking = false)
        {
            return await _unitRepo.chatRepo.GetChatAsync(userId, chatId);
        }

        public async Task<bool> IsExistAsync(int userID, int chatId)
        {
            return await _unitRepo.chatRepo.IsExistAsync(userID, chatId);
        }

        public async Task<Chat> AddChatAsync(User user, ChatDto chatDto)
        {
            Chat chat = new Chat
            {
                UserId = user.Id,
                ChatName = chatDto.ChatName,
                ChatFolderPath = "",
                CreatedAt = DateTime.UtcNow
            };
            _unitRepo.chatRepo.AddChat(chat);
            await _unitRepo.SaveInDataBaseAsync();

            string userFolderName = $"{user.Id}_{UnitServices.SanitizeForPath(user.Fname)}_{UnitServices.SanitizeForPath(user.Lname)}";
            string chatFolderName = $"{chat.Id}_{UnitServices.SanitizeForPath(chat.ChatName)}";
            string chatFolderPath = Path.Combine(_basePath!, userFolderName, chatFolderName);
            Directory.CreateDirectory(chatFolderPath);
            chat.ChatFolderPath = chatFolderPath;
            await _unitRepo.SaveInDataBaseAsync();

            return chat;
        }

        public async Task<bool> UpdateChatName(Chat chat, ChatDto chatDto)
        {
            if (!string.IsNullOrWhiteSpace(chat.ChatFolderPath) && Directory.Exists(chat.ChatFolderPath))
            {
                string parentPath = Directory.GetParent(chat.ChatFolderPath)!.FullName;
                string newChatName = chatDto.ChatName?.Trim()!;
                if (string.IsNullOrWhiteSpace(newChatName))
                    return false;

                string newFolderName = $"{chat.Id}_{UnitServices.SanitizeForPath(newChatName)}";
                string newFolderPath = Path.Combine(parentPath, newFolderName);
                try
                {
                    if (!string.Equals(chat.ChatFolderPath, newFolderPath, StringComparison.OrdinalIgnoreCase))
                        Directory.Move(chat.ChatFolderPath, newFolderPath);
                    chat.ChatName = newChatName;
                    chat.ChatFolderPath = newFolderPath;
                    await _unitRepo.chatRepo.RenameChatAsync(chat);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
            }
            return false;
        }

        public async Task DeleteChatAsync(Chat chat)
        {
            _unitRepo.chatRepo.RemoveChat(chat);

            string? folderPath = chat.ChatFolderPath;

            if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
                Directory.Delete(folderPath, recursive: true);
        }

        public async Task<bool> AnyChatsAsync(int userId)
        {
            return await _unitRepo.chatRepo.AnyChatsAsync(userId);
        }
    }
}
