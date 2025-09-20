using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian
{
    public class ApplicationRole: IdentityRole<int>
    {
        public string Description { get; set; }
        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        public ApplicationRole()
        {
        }
    }
}
