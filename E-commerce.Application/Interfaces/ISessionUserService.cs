using E_commerce.Domian.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Interfaces
{
    public interface ISessionUserService
    {
        int? UserId { get; }
        public RoleSystem? Role { get; }
    }
}
