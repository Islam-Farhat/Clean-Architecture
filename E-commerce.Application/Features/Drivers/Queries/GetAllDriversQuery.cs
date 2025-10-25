using E_commerce.Application.Features.Drivers.Dtos;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Drivers.Queries
{
    public class GetAllDriversQuery : IRequest<List<GetDriversDto>>
    {
        public class GetAllDriversQueryHandler : IRequestHandler<GetAllDriversQuery, List<GetDriversDto>>
        {
            private readonly IGetCleanerContext _context;
            private readonly UserManager<ApplicationUser> _userManager;

            public GetAllDriversQueryHandler(IGetCleanerContext context, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<List<GetDriversDto>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
            {
                var drivers = await _userManager.GetUsersInRoleAsync(RoleSystem.Driver.ToString());

                if (drivers.Any())
                {
                    return drivers.Select(x => new GetDriversDto
                    {
                        Id = x.Id,
                        UserName = x.UserName
                    }).ToList();
                }

                return new List<GetDriversDto>();
            }
        }
    }
}
