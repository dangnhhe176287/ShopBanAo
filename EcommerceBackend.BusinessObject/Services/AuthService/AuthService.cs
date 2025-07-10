using EcommerceBackend.BusinessObject.Abstract.AuthAbstract;
using EcommerceBackend.BusinessObject.dtos.AuthDto;
using EcommerceBackend.DataAccess.Abstract.AuthAbstract;
using EcommerceBackend.DataAccess.Models;
using EcommerceBackend.DataAccess.Repository.AuthRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBackend.BusinessObject.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        public AuthService(IAuthRepository authRepository, IConfiguration configuration,
            IEmailService emailService, IOtpService otpService)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _emailService = emailService;
            _otpService = otpService;
        }

        public string GenerateJwtToken(UserDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.RoleName)

                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Hàm kiểm tra định dạng email hợp lệ
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public UserDto? ValidateUser(LoginRequestDto loginRequest)
        {

            var user = _authRepository.GetUserByEmail(loginRequest.Email);
            if (user == null || user.Password != loginRequest.Password || user.Status != 1 || user.IsDelete == true)
            {
                return null;
            }
            if (user.Status != 1 || user.IsDelete == true)
            {
                throw new UnauthorizedAccessException();

            }

            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                RoleName = user.Role.RoleName ?? "",
                UserName = user.UserName ?? "",
            };
        }

        public UserDto RegisterUser(string email, string password, string userName, string? phone, DateTime? dateOfBirth, string? address)
        {
            // Kiểm tra email đã tồn tại chưa
            var existingUser = _authRepository.GetUserByEmail(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already exists.");
            }

            // Tạo mới người dùng
            var user = new User
            {
                Email = email,
                Password = password,
                UserName = userName,
                Phone = phone,
                DateOfBirth = dateOfBirth,
                Address = address,
                RoleId = 3,
                CreateDate = DateTime.Now,
                Status = 1,
                IsDelete = false
            };
            var createdUser = _authRepository.CreateUser(user);
            if (createdUser == null)
            {
                throw new Exception();
            }
            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                RoleName = user.Role.RoleName ?? "",
                UserName = user.UserName ?? "",
            };
        }
        public UserDto RegisterUserByGoogle(string email, string password, string userName, string? phone, DateTime? dateOfBirth, string? address)
        {
            var existingUser = _authRepository.GetUserByEmail(email);
            if (existingUser != null)
            {
                // Just return existing user as UserDto
                return new UserDto
                {
                    UserId = existingUser.UserId,
                    Email = existingUser.Email,
                    RoleName = existingUser.Role?.RoleName ?? "",
                    UserName = existingUser.UserName ?? "",

                };
            }

            // Create new user
            var user = new User
            {
                Email = email,
                Password = password,
                UserName = userName,
                Phone = phone,
                DateOfBirth = dateOfBirth,
                Address = address,
                RoleId = 3,
                CreateDate = DateTime.Now,
                Status = 1,
                IsDelete = false
            };

            var createdUser = _authRepository.CreateUser(user);
            if (createdUser == null)
            {
                throw new Exception("Failed to create user.");
            }

            return new UserDto
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                RoleName = createdUser.Role?.RoleName ?? "",
                UserName = createdUser.UserName ?? "",

            };
        }



        public async Task<bool> SendOtpForPasswordResetAsync(string email)
        {
            try
            {
                // Kiểm tra user có tồn tại không
                var user = _authRepository.GetUserByEmail(email);
                if (user == null || user.Status != 1 || user.IsDelete == true)
                {
                    return false;
                }

                // Tạo OTP
                var otp = _otpService.GenerateOtp();

                // Lưu OTP vào cache
                _otpService.StoreOtp(email, otp);

                // Gửi email
                await _emailService.SendOtpEmailAsync(email, otp);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            try
            {
                return _otpService.ValidateOtp(email, otp);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            try
            {
                // Kiểm tra OTP
                if (!_otpService.ValidateOtp(email, otp))
                {
                    return false;
                }

                // Cập nhật mật khẩu
                var result = await _authRepository.UpdateUserPasswordAsync(email, newPassword);

                if (result)
                {
                    // Xóa OTP sau khi đặt lại mật khẩu thành công
                    _otpService.RemoveOtp(email);
                }

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

}
