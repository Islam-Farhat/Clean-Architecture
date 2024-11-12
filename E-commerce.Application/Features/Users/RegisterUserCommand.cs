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

namespace E_commerce.Application.Features.Users
{
    public class RegisterUserCommand : IRequest<Result<AuthResponseModel>>
    {
        public string Address { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public class CreateUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthResponseModel>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IConfiguration _configuration;
            private readonly JWT _jwt;
            public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, IConfiguration configuration,IOptions<JWT> jwt)
            {
                _userManager = userManager;
                _configuration = configuration;
                _jwt = jwt.Value;
            }
            public async Task<Result<AuthResponseModel>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
            {
                if (await _userManager.FindByEmailAsync(request.Email) != null)
                    return Result.Failure<AuthResponseModel>("Email is already Exists");

                if (await _userManager.FindByNameAsync(request.Username) is not null)
                    return Result.Failure<AuthResponseModel>("Username is already registered!");

                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    Address = request.Address
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Empty;

                    foreach (var error in result.Errors)
                        errors += $"{error.Description},";

                    return new AuthResponseModel { Message = errors };
                }

                await _userManager.AddToRoleAsync(user, "User");

                var jwtSecurityToken = await CreateJwtToken(user);

                return new AuthResponseModel
                {
                    Email = user.Email,
                    ExpiresOn = jwtSecurityToken.ValidTo,
                    IsAuthenticated = true,
                    Roles = new List<string> { "User" },
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    Username = user.UserName
                };
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
