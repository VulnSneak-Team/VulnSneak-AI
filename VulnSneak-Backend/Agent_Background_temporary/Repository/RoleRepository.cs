using Agent_Background_temporary.Models;
using Microsoft.EntityFrameworkCore;

namespace Agent_Background_temporary.Repository
{
    public class RoleRepository
    {
        private AppDbContext _context;
        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetRoleAsync(User user)
        {
            return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(R => R.Id == user.RoleId);
        }
        public string? GetRoleName(User user)
        {
            return _context.Roles.AsNoTracking()
                .Where(R => R.Id == user.RoleId)
                .Select(R => R.RoleName)
                .FirstOrDefault();

        }
    }
}
