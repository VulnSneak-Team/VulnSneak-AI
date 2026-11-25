using Agent_Background_temporary.Models;

namespace Agent_Background_temporary.Repository
{
    public class UnitRepositroy
    {
        private AppDbContext _context;

        public UnitRepositroy(AppDbContext context)
        {
            _context = context;
        }

        public UserRepository UserRepo
        {
            get
            {
                if (field == null)
                    return new UserRepository(_context);
                return field;
            }
        }

        public ChatRepository chatRepo
        {
            get
            {
                if (field == null)
                    return new ChatRepository(_context);
                return field;
            }
        }

        public RoleRepository roleRepo
        {
            get
            {
                if (field == null)
                    return new RoleRepository(_context);
                return field;
            }
        }

        public ScanSessionRepository sacnSessionRepo
        {
            get
            {
                if (field == null)
                    return new ScanSessionRepository(_context);
                return field;
            }
        }

        public VulnResultRepository vulnResultRepo
        {
            get
            {
                if (field == null)
                    return new VulnResultRepository(_context);
                return field;
            }
        }

        public FileReportRepository fileReportRepo
        {
            get
            {
                if (field == null)
                    return new FileReportRepository(_context);
                return field;
            }
        }

        public VulnTypeRepository VulnTypeRepo
        {
            get
            {
                if (field == null)
                    return new VulnTypeRepository(_context);
                return field;
            }
        }
        public async Task SaveInDataBaseAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
