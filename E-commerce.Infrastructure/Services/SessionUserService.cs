using E_commerce.Application.Interfaces;
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
        public SessionUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            int.TryParse(user?.FindFirstValue("UserId"), out int userId);
            UserId = userId;
        }

    }
}
