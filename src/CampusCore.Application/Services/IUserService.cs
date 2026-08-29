
using CampusCore.Application.DTOs.Users;

namespace CampusCore.Application.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetUsersAsync();

        Task<UserResponseDto> GetUserByIdAsync(int id);

        Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);

        Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto dto);

        Task<UserResponseDto> PatchUserAsync(int id, PatchUserDto dto);

        Task DeleteUserAsync(int id);

    }
}
