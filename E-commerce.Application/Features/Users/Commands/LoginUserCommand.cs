using CSharpFunctionalExtensions;
using E_commerce.Application.Features.Users.Dtos;
using E_commerce.Application.Services.Interfaces;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Users.Commands
{
    public class LoginUserCommand : IRequest<Result<AuthenticationResponseDto>>
    {
        public string Username { get; init; }
        public string Password { get; init; }

        private class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthenticationResponseDto>>
        {
            private readonly IGetCleanerContext _context;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly ITokenService _tokenService;

            public LoginUserCommandHandler(IGetCleanerContext context, UserManager<ApplicationUser> userManager, ITokenService tokenService)
            {
                _context = context;
                _userManager = userManager;
                _tokenService = tokenService;
            }

            public async Task<Result<AuthenticationResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByEmailAsync(request.Username);
                if (user == null)
                    return Result.Failure<AuthenticationResponseDto>($"Wrong email or password");

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!isPasswordValid)
                    return Result.Failure<AuthenticationResponseDto>($"Wrong email or password");

                var userRoles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                        {
                            new ("UserId", user.Id.ToString()),
                            new ("UserName", $"{user.UserName}"),
                            new ("Role", userRoles?.FirstOrDefault())
                        };

                var accessToken = _tokenService.GenerateAccessToken(claims);
                var refreshToken = _tokenService.GenerateRefreshToken();
                user.UpdateRefreshToken(refreshToken.Token, refreshToken.Expiration);
                var isUpdated = await _userManager.UpdateAsync(user);

                if (!isUpdated.Succeeded)
                    return Result.Failure<AuthenticationResponseDto>($"Email Or Password may be wrong");

                return Result.Success(new AuthenticationResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token
                });
            }
        }
    }
}
