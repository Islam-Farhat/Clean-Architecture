using E_commerce.Application.Interfaces;
using E_commerce.Domian.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Services
{
    public class SessionUserService : ISessionUserService
    {
        public int? UserId { get; }
        public RoleSystem? Role { get; }
        public SessionUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            int.TryParse(user?.FindFirstValue("UserId"), out int userId);
            UserId = userId;

            var roleString = user?.FindFirstValue(ClaimTypes.Role);
            if (Enum.TryParse<RoleSystem>(roleString, out var role))
            {
                Role = role;
            }
        }

    }
}
