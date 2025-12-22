using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Users.Dtos
{
    public class GetUserDetailsDto
    {
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
