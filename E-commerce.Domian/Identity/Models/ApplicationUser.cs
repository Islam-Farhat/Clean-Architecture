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
        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
