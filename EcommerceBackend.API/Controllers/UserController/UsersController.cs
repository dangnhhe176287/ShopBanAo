using EcommerceBackend.BusinessObject.dtos.UserDto;
using EcommerceBackend.BusinessObject.Services.UserService;
using EcommerceBackend.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceBackend.API.Controllers.UserController
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        [AllowAnonymous] // Nếu muốn public, nếu muốn bảo mật thì bỏ dòng này
        public IActionResult Get(int id)
        {
            var user = _service.GetById(id);
            if (user == null) return NotFound();
            // Không trả về password
            var userProfile = new
            {
                user.UserId,
                user.UserName,
                user.Email,
                user.Phone,
                user.DateOfBirth,
                user.Address,
                user.CreateDate,
                user.Status,
                user.RoleId
            };
            return Ok(userProfile);
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserDto user)
        {
            _service.Add(user);
            return CreatedAtAction(nameof(Get), new { id = user.UserId }, user);
        }

        [HttpPut("{id}")]
        [AllowAnonymous] // Cho phép update user mà không cần authentication
        public IActionResult Put(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                Console.WriteLine($"Updating user with ID: {id}");
                Console.WriteLine($"User data: {updateUserDto.UserName}, {updateUserDto.Email}, {updateUserDto.Phone}, {updateUserDto.Address}");
                
                if (id != updateUserDto.UserId) 
                {
                    Console.WriteLine($"ID mismatch: {id} != {updateUserDto.UserId}");
                    return BadRequest("ID mismatch");
                }
                
                // Convert UpdateUserDto to UserDto
                var userDto = new UserDto
                {
                    UserId = updateUserDto.UserId,
                    RoleId = updateUserDto.RoleId,
                    Email = updateUserDto.Email,
                    Password = updateUserDto.Password ?? "", // Use empty string if null
                    Phone = updateUserDto.Phone ?? "",
                    UserName = updateUserDto.UserName ?? "",
                    DateOfBirth = updateUserDto.DateOfBirth,
                    Address = updateUserDto.Address ?? "",
                    CreateDate = updateUserDto.CreateDate,
                    Status = updateUserDto.Status,
                    IsDelete = updateUserDto.IsDelete
                };
                
                _service.Update(userDto);
                Console.WriteLine("User updated successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return BadRequest($"Error updating user: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return NoContent();
        }
    }
}
