using CSharpFunctionalExtensions;
using E_commerce.Domian;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Users
{
    public class LoginUserQuery : IRequest<Result<AuthResponseModel>>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, Result<AuthResponseModel>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IConfiguration _configuration;

            public LoginUserQueryHandler(UserManager<ApplicationUser> userManager, IConfiguration configuration)
            {
                _userManager = userManager;
                _configuration = configuration;
            }
            public async Task<Result<AuthResponseModel>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
            {
                var authModel = new AuthResponseModel();
                var user = await _userManager.FindByNameAsync(request.UserName);

                if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    return Result.Failure<AuthResponseModel>("Email or Password is incorrect!");
                }

                var jwtSecurityToken = await CreateJwtToken(user);
                var rolesList = await _userManager.GetRolesAsync(user);

                authModel.IsAuthenticated = true;
                authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authModel.Email = user.Email;
                authModel.Username = user.UserName;
                authModel.ExpiresOn = jwtSecurityToken.ValidTo;
                authModel.Roles = rolesList.ToList();

                //return Result.Success(authModel);
                return authModel;
            }


            private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
            {
                var s = _configuration.GetSection("JWT:key").Value.ToString();
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
