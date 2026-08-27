

using System.ComponentModel.DataAnnotations;

namespace CampusCore.Application.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}
