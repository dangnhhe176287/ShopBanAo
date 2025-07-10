using EcommerceBackend.DataAccess.Abstract.AuthAbstract;
using EcommerceBackend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBackend.DataAccess.Repository.AuthRepository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly EcommerceDBContext _context;

        public AuthRepository(EcommerceDBContext context)
        {
            _context = context;
        }

        public User? GetUserByEmail(string email)
        {
            try
            {
                return _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Email == email);
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId && (u.IsDelete == null || u.IsDelete == false));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public User? CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return _context.Users
                .Include(u => u.Role) 
                .FirstOrDefault(u => u.UserId == user.UserId);
        }

        public async Task<bool> UpdateUserPasswordAsync(string email, string newPassword)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return false;

                user.Password = newPassword;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
