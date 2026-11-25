using Agent_Background_temporary.Models;
using Microsoft.EntityFrameworkCore;

namespace Agent_Background_temporary.Repository
{
    public class UserRepository
    {
        private AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserAsync(int id, bool Tracking = false)
        {
            if (!Tracking)
                return await _context.Users.AsNoTracking().FirstOrDefaultAsync(U => U.Id == id);
            return await _context.Users.FirstOrDefaultAsync(U => U.Id == id);
        }
        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public void RemoveUser(User user)
        {
            _context.Users.Remove(user);
        }

        public void EditUser(User user)
        {
            _context.Users.Entry(user).State = EntityState.Modified;
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
        }

        internal async Task<User?> GetUserByEmailAsync(string Email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(U => U.Email == Email);
        }
    }
}
