using CSharpFunctionalExtensions;
using E_commerce.Application.Features.Users.Dtos;
using E_commerce.Application.Services.Interfaces;
using E_commerce.Domian.Entities;
using E_commerce.Domian;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace E_commerce.Application.Features.Users.Commands
{
    public class RefreshTokenCommand : IRequest<Result<AuthenticationResponseDto>>
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }

        private class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResponseDto>>
        {
            private readonly IGetCleanerContext _context;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly ITokenService _tokenService;
            private readonly TimeProvider _datetime;

            public RefreshTokenCommandHandler(IGetCleanerContext context, UserManager<ApplicationUser> userManager, ITokenService tokenService, TimeProvider datetime)
            {
                _context = context;
                _userManager = userManager;
                _tokenService = tokenService;
                _datetime = datetime;
            }
            public async Task<Result<AuthenticationResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
            {
                ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

                var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Result.Failure<AuthenticationResponseDto>("Email is missing from the token");

                var user = await _userManager.FindByIdAsync(userId);
                if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= _datetime.GetUtcNow())
                    return Result.Failure<AuthenticationResponseDto>("Invalid client request");

                var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                user.UpdateRefreshToken(newRefreshToken.Token, newRefreshToken.Expiration);

                await _userManager.UpdateAsync(user);
                var isSaved = await _context.SaveChangesAsyncWithResult();
                if (isSaved.IsFailure)
                    return Result.Failure<AuthenticationResponseDto>($"Email Or Password may be wrong");

                return Result.Success<AuthenticationResponseDto>(new()
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token
                });
            }
        }
    }
}
