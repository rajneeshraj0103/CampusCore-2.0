using CampusCore.Application.DTOs.Users;
using CampusCore.Application.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CampusCore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            var user = await _userService.CreateUserAsync(dto);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Id },
                user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
        {
            var user = await _userService.UpdateUserAsync(id, dto);

            return Ok(user);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(
            int id, 
            JsonPatchDocument<PatchUserDto> patchDoc)
        {
            var dto = new PatchUserDto();

            patchDoc.ApplyTo(dto, ModelState);

            TryValidateModel(dto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.PatchUserAsync(id, dto);

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUserAsync(id);

            return NoContent();
        }
    }
}
