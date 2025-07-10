using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBackend.BusinessObject.Abstract.AuthAbstract
{
    public interface IOtpService
    {
        string GenerateOtp();
        void StoreOtp(string email, string otp);
        bool ValidateOtp(string email, string otp);
        void RemoveOtp(string email);
    }

    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _otpExpiry = TimeSpan.FromMinutes(5);

        public OtpService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            return (randomNumber % 1000000).ToString("D6");
        }

        public void StoreOtp(string email, string otp)
        {
            var cacheKey = $"otp_{email}";
            _cache.Set(cacheKey, otp, _otpExpiry);
        }

        public bool ValidateOtp(string email, string otp)
        {
            var cacheKey = $"otp_{email}";
            if (_cache.TryGetValue(cacheKey, out string? storedOtp))
            {
                return storedOtp == otp;
            }
            return false;
        }

        public void RemoveOtp(string email)
        {
            var cacheKey = $"otp_{email}";
            _cache.Remove(cacheKey);
        }
    }
}
