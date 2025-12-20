using CSharpFunctionalExtensions;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Drivers.Commands
{
    public class DriverDeliveredHousemaidCommand : IRequest<Result>
    {
        public int WorkingId { get; set; }
        public class DriverDeliveredHousemaidCommandHandler : IRequestHandler<DriverDeliveredHousemaidCommand, Result>
        {
            private readonly IGetCleanerContext _context;

            public DriverDeliveredHousemaidCommandHandler(IGetCleanerContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(DriverDeliveredHousemaidCommand request, CancellationToken cancellationToken)
            {
                var workingDay = await _context.WorkingDays.AsTracking().FirstOrDefaultAsync(x=>x.Id == request.WorkingId);
                if (workingDay is null)
                    return Result.Failure("Id not exists.");


                workingDay.ChangeStatusToDeliveredToHome();

                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure<int>("Save Error");

                return Result.Success();

            }
        }
    }
}
