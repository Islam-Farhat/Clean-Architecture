using CSharpFunctionalExtensions;
using E_commerce.Application.Features.Users.Dtos;
using E_commerce.Application.Interfaces;
using E_commerce.Domian;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Users.Queries
{
    public class GetUserDetailsQuery : IRequest<Result<GetUserDetailsDto>>
    {
        public class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, Result<GetUserDetailsDto>>
        {
            private readonly ISessionUserService _sessionUser;
            private readonly UserManager<ApplicationUser> _userManager;

            public GetUserDetailsQueryHandler(UserManager<ApplicationUser> userManager, ISessionUserService sessionUser)
            {
                _userManager = userManager;
                _sessionUser = sessionUser;
            }

            public async Task<Result<GetUserDetailsDto>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByIdAsync(_sessionUser?.UserId.ToString());
                if (user == null)
                    return Result.Failure<GetUserDetailsDto>("User not found.");

                var userRoles = await _userManager.GetRolesAsync(user);

                var userDto = new GetUserDetailsDto
                {
                    PhoneNumber = user.PhoneNumber,
                    Username = user.UserName,
                    Role = string.Join(",", userRoles)
                };

                return userDto;
            }
        }
    }
}
