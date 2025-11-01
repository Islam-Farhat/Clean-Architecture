using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using E_commerce.Domian;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static CSharpFunctionalExtensions.Result;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using E_commerce.Application.Helper;
using Microsoft.Extensions.Options;
using E_commerce.Domian.Entities;
using System.Security.Cryptography;
using E_commerce.Domian.Enums;

namespace E_commerce.Application.Features.Users.Commands
{
    public class RegisterUserCommand : IRequest<Result>
    {
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }


        public class CreateUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<ApplicationRole> _roleManager;
            private readonly IConfiguration _configuration;
            private readonly JWT _jwt;
            public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration, IOptions<JWT> jwt)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _configuration = configuration;
                _jwt = jwt.Value;
            }
            public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
            {
                if (!Enum.IsDefined(typeof(RoleSystem), request.Role))
                    return Result.Failure($"Invalid role.");

                var roleName = ((RoleSystem)request.Role).ToString();

                if (!await _roleManager.RoleExistsAsync(roleName))
                    return Result.Failure($"Role {roleName} does not exist");


                if (await _userManager.FindByNameAsync(request.Username) is not null)
                    return Failure("Username is already registered!");

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
                    return Result.Failure(errors);
                }
                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return Result.Failure($"Failed to assign role: {errors}");
                }

                return Result.Success();
            }

            private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
            {
                var secretkey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("JWT:key").Value.ToString()));

                var signcredentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256Signature);

                var userClaims = await _userManager.GetClaimsAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                var roleClaims = new List<Claim>();
                foreach (var role in roles)
                    roleClaims.Add(new Claim("roles", role));
                var claims = new[]
                       {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email),
                            new Claim("UserId", user.Id.ToString())
                        }
                       .Union(userClaims)
                       .Union(roleClaims);
                var expireDate = DateTime.Now.AddDays(1);
                var jwtSecurityToken = new JwtSecurityToken(
                    claims: claims,
                    expires: expireDate,
                    signingCredentials: signcredentials);

                return jwtSecurityToken;
            }
        }
    }
}
