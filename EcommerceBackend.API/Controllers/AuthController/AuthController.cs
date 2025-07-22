using EcommerceBackend.BusinessObject.Abstract.AuthAbstract;
using EcommerceBackend.BusinessObject.dtos.AuthDto;
using EcommerceBackend.BusinessObject.dtos.UserDto;
using EcommerceBackend.BusinessObject.Services.UserService;
using EcommerceBackend.DataAccess.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceBackend.API.Controllers.AuthController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                if (loginRequest == null)
                {
                    return BadRequest(new { message = "Invalid request data." });
                }

                if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    return BadRequest(new { message = "Email and Password are required." });
                }

                if (loginRequest.Email.Length > 255)
                {
                    return BadRequest(new { message = "Email must be at most 255 characters long." });
                }

                if (!_authService.IsValidEmail(loginRequest.Email))
                {
                    return BadRequest(new { message = "Invalid email format." });
                }

                if (loginRequest.Password.Length > 255)
                {
                    return BadRequest(new { message = "Password must be at most 255 characters long." });
                }

                var user = _authService.ValidateUser(loginRequest);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email, password." });
                }

                var token = _authService.GenerateJwtToken(user);
                var session = _httpContextAccessor.HttpContext.Session;
                session.SetString("Token", token);
                session.SetString("UserId", user.UserId.ToString());
                session.SetString("Email", user.Email);

                return Ok(new { message = "successful", token, userId = user.UserId, RoleName = user.RoleName, UserName = user.UserName });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "deactived account." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while processing your request.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequestDto registerRequest)
        {
            if (registerRequest == null)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            if (string.IsNullOrWhiteSpace(registerRequest.Email) || string.IsNullOrWhiteSpace(registerRequest.Password) || string.IsNullOrWhiteSpace(registerRequest.UserName))
            {
                return BadRequest(new { message = "Email, Password, and UserName are required." });
            }

            if (registerRequest.Email.Length > 255)
            {
                return BadRequest(new { message = "Email must be at most 255 characters long." });
            }

            if (registerRequest.Password.Length > 255)
            {
                return BadRequest(new { message = "Password must be at most 255 characters long." });
            }

            if (registerRequest.UserName.Length > 255)
            {
                return BadRequest(new { message = "UserName must be at most 255 characters long." });
            }

            if (registerRequest.Phone != null && registerRequest.Phone.Length > 20)
            {
                return BadRequest(new { message = "Phone must be at most 20 characters long." });
            }

            if (registerRequest.Address != null && registerRequest.Address.Length > 500)
            {
                return BadRequest(new { message = "Address must be at most 500 characters long." });
            }

            if (!_authService.IsValidEmail(registerRequest.Email))
            {
                return BadRequest(new { message = "Email address does not match format." });
            }

            try
            {
                var user = _authService.RegisterUser(
                    registerRequest.Email,
                    registerRequest.Password,
                    registerRequest.UserName,
                    registerRequest.Phone,
                    registerRequest.DateOfBirth,
                    registerRequest.Address
                );

                var token = _authService.GenerateJwtToken(user);
                var session = _httpContextAccessor.HttpContext.Session;
                session.SetString("Token", token);
                session.SetString("UserId", user.UserId.ToString());
                session.SetString("Email", user.Email);

                return Ok(new { message = "successful", token, userId = user.UserId, RoleName = user.RoleName, UserName = user.UserName });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi không xác định.", error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required." });
                }

                if (!_authService.IsValidEmail(request.Email))
                {
                    return BadRequest(new { message = "Invalid email format." });
                }

                var result = await _authService.SendOtpForPasswordResetAsync(request.Email);

                if (result)
                {
                    return Ok(new { message = "OTP has been sent to your email." });
                }
                else
                {
                    return BadRequest(new { message = "Email not found or account is inactive." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while sending OTP.", error = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.OtpCode))
                {
                    return BadRequest(new { message = "Email and OTP code are required." });
                }

                if (!_authService.IsValidEmail(request.Email))
                {
                    return BadRequest(new { message = "Invalid email format." });
                }

                if (request.OtpCode.Length != 6)
                {
                    return BadRequest(new { message = "OTP code must be 6 digits." });
                }

                var result = await _authService.VerifyOtpAsync(request.Email, request.OtpCode);

                if (result)
                {
                    return Ok(new { message = "OTP verified successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Invalid or expired OTP code." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while verifying OTP.", error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.OtpCode) || string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { message = "Email, OTP code, and new password are required." });
                }

                if (!_authService.IsValidEmail(request.Email))
                {
                    return BadRequest(new { message = "Invalid email format." });
                }

                if (request.OtpCode.Length != 6)
                {
                    return BadRequest(new { message = "OTP code must be 6 digits." });
                }

                if (request.NewPassword.Length < 6 || request.NewPassword.Length > 255)
                {
                    return BadRequest(new { message = "Password must be between 6 and 255 characters." });
                }

                var result = await _authService.ResetPasswordAsync(request.Email, request.OtpCode, request.NewPassword);

                if (result)
                {
                    return Ok(new { message = "Password has been reset successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Invalid or expired OTP code." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while resetting password.", error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (request == null || request.UserId <= 0 || string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "UserId, old password, and new password are required." });
            }
            if (request.NewPassword.Length < 6 || request.NewPassword.Length > 255)
            {
                return BadRequest(new { message = "New password must be between 6 and 255 characters." });
            }
            try
            {
                _userService.ChangePassword(request.UserId, request.OldPassword, request.NewPassword);
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("google-login")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse));
            var props = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public IActionResult GoogleResponse()
        {
            var resultTask = HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            resultTask.Wait();
            var result = resultTask.Result;

            if (!result.Succeeded || result.Principal == null)
                return Redirect("https://localhost:7257/LoginPage/Login?error=Google authentication failed");

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var fullName = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
                return Redirect("https://localhost:7257/LoginPage/Login?error=No email returned by Google");

            var user = _authService.RegisterUserByGoogle(
                email,
                "123",
                fullName ?? email.Split('@')[0],
                "",
                null,
                ""
            );

            var token = _authService.GenerateJwtToken(user);

            return Redirect($"https://localhost:7107/LoginPage/Login?token={token}&userName={Uri.EscapeDataString(user.UserName)}&roleName={Uri.EscapeDataString(user.RoleName)}");
        }
    }
}
