using CSharpFunctionalExtensions;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Users
{
    public class RefreshTokenQuery : IRequest<Result<AuthResponseModel>>
    {
        public string RefreshToken { get; set; }
        public class RefreshTokenQueryHandler : IRequestHandler<RefreshTokenQuery, Result<AuthResponseModel>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IConfiguration _configuration;

            public RefreshTokenQueryHandler(UserManager<ApplicationUser> userManager, IConfiguration configuration)
            {
                _userManager = userManager;
                _configuration = configuration;
            }
            public async Task<Result<AuthResponseModel>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
            {
                var authModel = new AuthResponseModel();

                var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == request.RefreshToken));

                if (user == null)
                {
                    authModel.Message = "Invalid token";
                    return authModel;
                }

                var refreshToken = user.RefreshTokens.Single(t => t.Token == request.RefreshToken);

                if (!refreshToken.IsActive)
                {
                    authModel.Message = "Inactive token";
                    return authModel;
                }

                refreshToken.RevokedOn = DateTime.UtcNow;

                var newRefreshToken = GenerateRefreshToken();
                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);

                var jwtToken = await CreateJwtToken(user);
                authModel.IsAuthenticated = true;
                authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                authModel.Email = user.Email;
                authModel.Username = user.UserName;
                var roles = await _userManager.GetRolesAsync(user);
                authModel.Roles = roles.ToList();
                authModel.RefreshToken = newRefreshToken.Token;
                authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

                return authModel;
            }

            private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
            {
                var key = _configuration.GetSection("JWT:key").Value.ToString();
                var secretkey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

                var signcredentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256Signature);

                var userClaims = await _userManager.GetClaimsAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                var roleClaims = new List<Claim>();
                foreach (var role in roles)
                    roleClaims.Add(new Claim("Roles", role));
                var claims = new[]
                       {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email),
                            new Claim("UserId", user.Id.ToString())
                        }
                       .Union(userClaims)
                       .Union(roleClaims);
                var expireDate = DateTime.Now.AddMinutes(1);
                var jwtSecurityToken = new JwtSecurityToken(
                    claims: claims,
                    expires: expireDate,
                    signingCredentials: signcredentials);

                return jwtSecurityToken;
            }

            private RefreshToken GenerateRefreshToken()
            {
                var randomNumber = new byte[32];

                using var generator = new RNGCryptoServiceProvider();

                generator.GetBytes(randomNumber);

                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomNumber),
                    ExpiresOn = DateTime.UtcNow.AddDays(10),
                    CreatedOn = DateTime.UtcNow
                };
            }
        }
    }
}
