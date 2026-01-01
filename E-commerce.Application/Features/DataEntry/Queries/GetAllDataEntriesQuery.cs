using E_commerce.Application.Features.Drivers.Dtos;
using E_commerce.Application.Features.Drivers.Queries;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using E_commerce.Domian;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Application.Features.DataEntry.Dtos;

namespace E_commerce.Application.Features.DataEntry.Queries
{
    public class GetAllDataEntriesQuery : IRequest<List<GetDataEntryDto>>
    {

        public class GetAllDataEntriesQueryHandler : IRequestHandler<GetAllDataEntriesQuery, List<GetDataEntryDto>>
        {
            private readonly IGetCleanerContext _context;
            private readonly UserManager<ApplicationUser> _userManager;

            public GetAllDataEntriesQueryHandler(IGetCleanerContext context, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<List<GetDataEntryDto>> Handle(GetAllDataEntriesQuery request, CancellationToken cancellationToken)
            {
                var drivers = await _userManager.GetUsersInRoleAsync(RoleSystem.DataEntry.ToString());

                if (drivers.Any())
                {
                    return drivers.Select(x => new GetDataEntryDto
                    {
                        Id = x.Id,
                        UserName = x.UserName
                    }).ToList();
                }

                return new List<GetDataEntryDto>();
            }
        }
    }
}
