using CSharpFunctionalExtensions;
using E_commerce.Application.Features.Users.Commands;
using E_commerce.Application.Helper;
using E_commerce.Domian.Enums;
using E_commerce.Domian;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Drivers.Commands
{
    public class AddDriverCommand : IRequest<Result<int>>
    {
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public class AddDriverCommandHandler : IRequestHandler<AddDriverCommand, Result<int>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<ApplicationRole> _roleManager;
            private readonly IConfiguration _configuration;
            private readonly JWT _jwt;
            public AddDriverCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration, IOptions<JWT> jwt)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _configuration = configuration;
                _jwt = jwt.Value;
            }
            public async Task<Result<int>> Handle(AddDriverCommand request, CancellationToken cancellationToken)
            {
                if (await _userManager.FindByNameAsync(request.Username) is not null)
                    return Result.Failure<int>("Username is already registered!");

                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Username,
                    PhoneNumber = request.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure<int>(errors);
                }
                var roleResult = await _userManager.AddToRoleAsync(user, RoleSystem.Driver.ToString());
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return Result.Failure<int>($"Failed to assign role: {errors}");
                }

                return Result.Success(user.Id);
            }

        }
    }
}
