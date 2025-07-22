using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.BusinessObject.dtos.UserDto
{
    public class ChangePasswordRequestDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string OldPassword { get; set; } = string.Empty;
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}