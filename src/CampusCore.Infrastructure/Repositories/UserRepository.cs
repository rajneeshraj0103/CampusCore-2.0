
using CampusCore.Application.Interfaces;
using CampusCore.Domain.Entities;
using CampusCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CampusCore.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CampusCoreDbContext _context;
        public UserRepository(CampusCoreDbContext context)
        {
            _context = context;
        }
        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
