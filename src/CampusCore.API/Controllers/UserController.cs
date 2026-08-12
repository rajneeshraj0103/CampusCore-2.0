using CampusCore.Application.Services;
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

        [HttpGet("test")]
        public IActionResult Test()
        {
            var message = _userService.GetMessage();
            return Ok(message);
        }
    }
}
