using E_commerce.Domian.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string Address { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }


        public void UpdateRefreshToken(string newRefreshToken, DateTime refreshTokenExpiryTime)
        {
            if (string.IsNullOrWhiteSpace(newRefreshToken))
                throw new ArgumentException("Refresh Token cannot be empty");

            RefreshToken = newRefreshToken;
            RefreshTokenExpiryTime = refreshTokenExpiryTime;
        }

    }
}
