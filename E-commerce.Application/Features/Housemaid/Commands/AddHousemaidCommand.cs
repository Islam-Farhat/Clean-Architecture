using CSharpFunctionalExtensions;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Housemaids.Commands
{
    public class AddHousemaidCommand : IRequest<Result<int>>
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string? ImageBase64 { get; set; }


        public class AddHousemaidCommandHandler : IRequestHandler<AddHousemaidCommand, Result<int>>
        {
            private readonly IMediaService _mediaService;
            private readonly IGetCleanerContext _context;
            public AddHousemaidCommandHandler(IMediaService mediaService, IGetCleanerContext context)
            {
                _mediaService = mediaService;
                _context = context;
            }

            public async Task<Result<int>> Handle(AddHousemaidCommand request, CancellationToken cancellationToken)
            {

                var housemaid = Domian.Entities.Housemaid.Instance(request.Name, request.Address, request.PhoneNumber);

                if (!string.IsNullOrWhiteSpace(request.ImageBase64) )
                {
                    var fileName = await _mediaService.UploadImage(request.ImageBase64, "ImageBank/Housemaid");
                    if (string.IsNullOrWhiteSpace(fileName.Value))
                        return Result.Failure<int>("Upload Image Failed");

                    housemaid.Value.UpdateImage(fileName.Value);
                }

                if (housemaid.IsFailure)
                    return Result.Failure<int>(housemaid.Error);

                await _context.Housemaids.AddAsync(housemaid.Value);
                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure<int>("Save Error");

                return Result.Success<int>(housemaid.Value.Id);

            }
        }
    }
}
