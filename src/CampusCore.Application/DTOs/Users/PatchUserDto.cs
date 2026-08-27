
using System.ComponentModel.DataAnnotations;

namespace CampusCore.Application.DTOs.Users
{
    public class PatchUserDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [Range(1, int.MaxValue)]
        public int? RoleId { get; set; }
    }
}
