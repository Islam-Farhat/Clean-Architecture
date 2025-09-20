using E_commerce.Application.Interfaces;
using E_commerce.Domian;
using E_commerce.Domian.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Seeding
{
    public class RoleSeedService : IRoleSeedService
    {

        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleSeedService(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task SeedRolesAsync()
        {
            var roleNames = Enum.GetNames(typeof(RoleSystem));
            var roleDescriptions = new Dictionary<string, string>
            {
                { RoleSystem.Admin.ToString(), "Full access to all system features and settings" },
                { RoleSystem.Supervisor.ToString(), "Manages drivers and oversees operations" },
                { RoleSystem.Driver.ToString(), "Performs delivery and transportation tasks" },
                { RoleSystem.DataEntry.ToString(), "Responsible for entering and managing data" }
            };

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole(roleName)
                    {
                        Description = roleDescriptions.ContainsKey(roleName) ? roleDescriptions[roleName] : string.Empty
                    };
                    await _roleManager.CreateAsync(role);
                }
            }
        }
    }
}
