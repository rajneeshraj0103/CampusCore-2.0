
using CampusCore.Application.DTOs.Users;
using CampusCore.Application.Exceptions;
using CampusCore.Application.Interfaces;
using CampusCore.Domain.Entities;

namespace CampusCore.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly IRoleRepository _roleRepository;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }


        public async Task<List<UserResponseDto>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(user => new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                CreatedAt = user.CreatedAt
            }).ToList();
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user =  await _userRepository.GetByIdAsync(id);

            if(user == null)
            {
                throw new NotFoundException($"User with Id {id} was not found.");
            }

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            var role = await _roleRepository.GetByIdAsync(dto.RoleId);

            if (role == null)
            {
                throw new BadRequestException("Invalid RoleId.");
            }

            var user = new User()
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = dto.Password,
                RoleId = dto.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserResponseDto> UpdateUserAsync(
            int id, 
            UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                throw new NotFoundException($"User with Id {id} was not found.");
            }

            var role = await _roleRepository.GetByIdAsync(dto.RoleId);

            if (role == null)
            {
                throw new BadRequestException("Invalid RoleId.");
            }

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.RoleId = dto.RoleId;

            await _userRepository.UpdateAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserResponseDto> PatchUserAsync(int id, PatchUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if(user == null)
            {
                throw new NotFoundException($"User with Id {id} was not found.");
            }

            if(dto.Name != null)
            {
                user.Name = dto.Name;
            }

            if (dto.Email != null)
            {
                user.Email = dto.Email;
            }

            if (dto.RoleId.HasValue)
            {
                var role = await _roleRepository.GetByIdAsync(dto.RoleId.Value);

                if (role == null)
                {
                    throw new BadRequestException("Invalid RoleId.");
                }

                user.RoleId = dto.RoleId.Value;
            }

            await _userRepository.UpdateAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if(user == null)
            {
                throw new NotFoundException($"User with Id {id} was not found.");
            }

            await _userRepository.DeleteAsync(user);
        }
    }
}
