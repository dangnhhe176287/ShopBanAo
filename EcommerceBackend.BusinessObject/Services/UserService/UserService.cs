using EcommerceBackend.BusinessObject.dtos.UserDto;
using EcommerceBackend.DataAccess.Models;
using EcommerceBackend.DataAccess.Repository.UserRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBackend.BusinessObject.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<User> GetAll() => _repo.GetAll();
        public User? GetById(int id) => _repo.GetById(id);
        public User? GetByEmail(string email) => _repo.GetByEmail(email);
        public void Add(UserDto userDto)
        {
            var user = new User
            {
                //UserId = userDto.UserId,
                RoleId = userDto.RoleId,
                Email = userDto.Email,
                Password = userDto.Password,
                Phone = userDto.Phone,
                UserName = userDto.UserName,
                DateOfBirth = userDto.DateOfBirth,
                Address = userDto.Address,
                CreateDate = userDto.CreateDate == default ? DateTime.Now : userDto.CreateDate,
                Status = userDto.Status,
                IsDelete = userDto.IsDelete
            };
            _repo.Add(user);
        }
        public void Update(UserDto userDto)
        {
            var user = _repo.GetById(userDto.UserId);
            if (user == null)
                throw new Exception($"User with ID {userDto.UserId} not found");

            user.RoleId = userDto.RoleId;
            user.Email = userDto.Email;
            
            // Chỉ update password nếu có giá trị
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                user.Password = userDto.Password;
            }
            
            user.Phone = userDto.Phone;
            user.UserName = userDto.UserName;
            user.DateOfBirth = userDto.DateOfBirth;
            user.Address = userDto.Address;
            user.Status = userDto.Status;
            user.IsDelete = userDto.IsDelete;

            _repo.Update(user);
        }

        public void Delete(int id) => _repo.Delete(id);

        public void ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = _repo.GetById(userId);
            if (user == null)
                throw new Exception($"User with ID {userId} not found");
            if (user.Password != oldPassword)
                throw new Exception("Old password is incorrect");
            user.Password = newPassword;
            _repo.Update(user);
        }
    }

}
