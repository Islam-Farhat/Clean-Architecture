using CSharpFunctionalExtensions;
using E_commerce.Application.Features.Housemaid.Dtos;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Housemaid.Queries
{
    public class GetHousemaidsCommand : IRequest<List<GetHousemaidDto>>
    {
        public class GetHousemaidsCommandHandler : IRequestHandler<GetHousemaidsCommand, List<GetHousemaidDto>>
        {
            private readonly IMediaService _mediaService;
            private readonly IGetCleanerContext _context;
            private readonly IConfiguration _configuration;
            public GetHousemaidsCommandHandler(IMediaService mediaService, IGetCleanerContext context, IConfiguration configuration)
            {
                _mediaService = mediaService;
                _context = context;
                _configuration = configuration;
            }
            public async Task<List<GetHousemaidDto>> Handle(GetHousemaidsCommand request, CancellationToken cancellationToken)
            {
                var baseUrl = _configuration["GetCleaner:BaseUrl"];

                var housemaids = await _context.Housemaids.Select(x => new GetHousemaidDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    ImagePath = $"{baseUrl}ImageBank/Housemaid/{x.ImageUrl}"
                }).ToListAsync();

                return housemaids;
            }

        }
    }
}
