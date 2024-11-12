using E_commerce.Application.Helper;
using E_commerce.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private SymmetricSecurityKey _secretKey;
        private readonly TimeProvider _datetime;
        private readonly IConfiguration _configuration;
        private readonly JWT _jwt;

        public TokenService(TimeProvider datetime, IConfiguration configuration, IOptions<JWT> jwt)
        {
            _datetime = datetime;
            _configuration = configuration;
            _jwt = jwt.Value;
            _secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]!));

        }
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var signinCredentials = new SigningCredentials(_secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: _datetime.GetLocalNow().Date.AddSeconds(_jwt.ExpiryInMinutes),
                signingCredentials: signinCredentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;
        }

        public (string Token, DateTime Expiration) GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var refreshTokenExpiryTime = _datetime.GetUtcNow()
                .AddDays(_jwt.RefreshTokenExpiryInDays)
                .UtcDateTime;

            return (Convert.ToBase64String(randomNumber), refreshTokenExpiryTime);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _secretKey,
                ValidateLifetime = false,
                ValidIssuer = _jwt.Issuer,
                ValidAudience = _jwt.Audience,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
