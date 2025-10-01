using CSharpFunctionalExtensions;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Housemaid.Commands
{
    public class UpdateHousemaidCommand : IRequest<Result>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string? ImageBase64 { get; set; }

        public class UpdateHousemaidCommandHandler : IRequestHandler<UpdateHousemaidCommand, Result>
        {
            private readonly IMediaService _mediaService;
            private readonly IGetCleanerContext _context;
            public UpdateHousemaidCommandHandler(IMediaService mediaService, IGetCleanerContext context)
            {
                _mediaService = mediaService;
                _context = context;
            }
            public async Task<Result> Handle(UpdateHousemaidCommand request, CancellationToken cancellationToken)
            {
                string base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

                var housemaid = await _context.Housemaids.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id);

                if (housemaid == null)
                    return Result.Failure("Housemaid not found!");

                if (!string.IsNullOrWhiteSpace(request.ImageBase64))
                {
                    var fileName = await _mediaService.UploadImage(request.ImageBase64, "ImageBank/Housemaid");
                    if (string.IsNullOrWhiteSpace(fileName.Value))
                        return Result.Failure<int>("Upload Image Failed");

                    housemaid.UpdateImage(fileName.Value);
                }

                var resultUpdate = housemaid.Update(request.Name, request.Address, request.PhoneNumber);
                if (resultUpdate.IsFailure)
                    return Result.Failure<int>(resultUpdate.Error);

                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure<int>("Save Error");

                return Result.Success();

            }
        }
    }
}
