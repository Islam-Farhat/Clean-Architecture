using CSharpFunctionalExtensions;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Drivers.Commands
{
    public class DriverEndOfShiftCommand : IRequest<Result>
    {
        public int Id { get; set; }
        public PaymentType? PaymentType { get; set; }
        public string? Comments { get; set; }
        public decimal? Amount { get; set; }
        public string? PaymentImageBase64 { get; set; }

        public class DriverEndOfShiftCommandHandler : IRequestHandler<DriverEndOfShiftCommand, Result>
        {
            private readonly IGetCleanerContext _context;
            private readonly IMediaService _mediaService;

            public DriverEndOfShiftCommandHandler(IGetCleanerContext context, IMediaService mediaService)
            {
                _context = context;
                _mediaService = mediaService;
            }

            public async Task<Result> Handle(DriverEndOfShiftCommand request, CancellationToken cancellationToken)
            {
                var workingDay = await _context.WorkingDays.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id);
                if (workingDay is null)
                    return Result.Failure("Id not exists.");

                if (!string.IsNullOrWhiteSpace(request.PaymentImageBase64))
                {
                    var fileName = await _mediaService.UploadImage(request.PaymentImageBase64, "ImageBank/WorkingDay");
                    if (string.IsNullOrWhiteSpace(fileName.Value))
                        return Result.Failure<int>("Upload Image Failed");

                    workingDay.UpdatePaymentImage(fileName.Value);
                }

                workingDay.ChangeStatusToEndOfShift(request.PaymentType,request.Amount,request.Comments);

                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure<int>("Save Error");

                return Result.Success();
            }
        }
    }
}
