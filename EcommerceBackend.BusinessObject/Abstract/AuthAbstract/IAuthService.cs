﻿using EcommerceBackend.BusinessObject.dtos.AuthDto;
using EcommerceBackend.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBackend.BusinessObject.Abstract.AuthAbstract
{
    public interface IAuthService
    {
        public string GenerateJwtToken(UserDto user);
        bool IsValidEmail(string email);
        UserDto? ValidateUser(LoginRequestDto loginRequest);
        UserDto RegisterUser(string email, string password, string userName, string? phone, DateTime? dateOfBirth, string? address);

        UserDto RegisterUserByGoogle(string email, string password, string userName, string? phone, DateTime? dateOfBirth, string? address);



        Task<bool> SendOtpForPasswordResetAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);
    }
}
