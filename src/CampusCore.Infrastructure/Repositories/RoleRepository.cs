
using CampusCore.Application.Interfaces;
using CampusCore.Domain.Entities;
using CampusCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CampusCore.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly CampusCoreDbContext _context;

        public RoleRepository(CampusCoreDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
