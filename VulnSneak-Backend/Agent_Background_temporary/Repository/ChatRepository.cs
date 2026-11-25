using Agent_Background_temporary.Models;
using Microsoft.EntityFrameworkCore;

namespace Agent_Background_temporary.Repository
{
    public class ChatRepository
    {
        private AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Chat>> GetAllChatsAsync(int userId)
        {
            return await _context.Chats.AsNoTracking()
                .Where(C => C.UserId == userId)
                .OrderByDescending(C => C.CreatedAt)
                .ToListAsync();
        }

        public async Task<Chat?> GetChatAsync(int userId, int chatId, bool Tracking = false)
        {
            if (!Tracking)
                return await _context.Chats.AsNoTracking().FirstOrDefaultAsync(C => C.Id == chatId && C.UserId == userId);
            return await _context.Chats.FirstOrDefaultAsync(C => C.Id == chatId && C.UserId == userId);
        }

        public void AddChat(Chat chat)
        {
            _context.Chats.Add(chat);
        }
        public async Task RenameChatAsync(Chat chat)
        {
            _context.Entry(chat).State = EntityState.Modified;

            //return await _context.Chats
            //    .Where(C => C.Id == chatDto.ChatId && C.UserId == userId)
            //    .ExecuteUpdateAsync(S =>
            //        S.SetProperty(C => C.ChatName, chatDto.ChatName).
            //        SetProperty(C => C.)

            //        );
        }

        public void RemoveChat(Chat chat)
        {
            _context.Chats.Remove(chat);
        }

        public async Task<bool> IsExistAsync(int userId, int chatId)
        {
            return await _context.Chats.AsNoTracking().AnyAsync(C => C.Id == chatId && C.UserId == userId);
        }

        public async Task<bool> AnyChatsAsync(int userId)
        {
            return await _context.Chats.AnyAsync(C => C.UserId == userId);
        }
    }
}
