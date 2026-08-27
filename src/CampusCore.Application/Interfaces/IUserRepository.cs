
using CampusCore.Domain.Entities;

namespace CampusCore.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();

        Task<User?> GetByIdAsync(int id);

        Task<User> AddAsync(User user);

        Task<User> UpdateAsync(User user);

        Task<bool> DeleteAsync(User user);
    }
}
