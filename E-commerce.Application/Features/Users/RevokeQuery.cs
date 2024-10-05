using CSharpFunctionalExtensions;
using E_commerce.Domian;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Users
{
    public class RevokeQuery : IRequest<bool>
    {
        public string RefreshToken { get; set; }

        public class RevokeQueryHandler : IRequestHandler<RevokeQuery, bool>
        {
            private readonly UserManager<ApplicationUser> _userManager;

            public RevokeQueryHandler(UserManager<ApplicationUser> userManager)
            {
                _userManager = userManager;
            }
            public async Task<bool> Handle(RevokeQuery request, CancellationToken cancellationToken)
            {
                var authModel = new AuthResponseModel();

                var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == request.RefreshToken));

                if (user == null)
                    return false;

                var refreshToken = user.RefreshTokens.Single(t => t.Token == request.RefreshToken);

                if (!refreshToken.IsActive)
                    return false;

                refreshToken.RevokedOn = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return true;
            }
        }
    }
}
