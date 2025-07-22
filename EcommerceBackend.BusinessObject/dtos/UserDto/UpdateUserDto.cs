using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.BusinessObject.dtos.UserDto
{
    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        // Password không bắt buộc khi update profile
        public string? Password { get; set; }

        public string? Phone { get; set; }
        public string? UserName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Address { get; set; }

        public DateTime CreateDate { get; set; }

        public int Status { get; set; }

        public bool IsDelete { get; set; }
    }
} 