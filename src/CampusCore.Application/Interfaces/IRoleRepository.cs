
using CampusCore.Domain.Entities;

namespace CampusCore.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int id);
    }
}
