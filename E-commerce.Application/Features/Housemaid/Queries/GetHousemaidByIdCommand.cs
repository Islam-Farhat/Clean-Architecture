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
    public class GetHousemaidByIdCommand : IRequest<Result<GetHousemaidDto>>
    {
        public int Id { get; set; }

        public class GetHousemaidByIdCommandHandler : IRequestHandler<GetHousemaidByIdCommand, Result<GetHousemaidDto>>
        {
            private readonly IMediaService _mediaService;
            private readonly IGetCleanerContext _context;
            private readonly IConfiguration _configuration;
            public GetHousemaidByIdCommandHandler(IMediaService mediaService, IGetCleanerContext context, IConfiguration configuration)
            {
                _mediaService = mediaService;
                _context = context;
                _configuration = configuration;
            }
            public async Task<Result<GetHousemaidDto>> Handle(GetHousemaidByIdCommand request, CancellationToken cancellationToken)
            {
                var baseUrl = _configuration["GetCleaner:BaseUrl"];

                var housemaid = await _context.Housemaids.Select(x=>new GetHousemaidDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    ImagePath = $"{baseUrl}ImageBank/Housemaid/{x.ImageUrl}"
                }).FirstOrDefaultAsync(x => x.Id == request.Id);

                if (housemaid == null)
                    return Result.Failure<GetHousemaidDto>("Housemaid not found!");

                return Result.Success<GetHousemaidDto>(housemaid);
            }
        }
    }
}
