using CSharpFunctionalExtensions;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Orders.Commands
{
    public class CancelOrderCommand : IRequest<Result>
    {
        public int Id { get; set; }
        public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
        {
            private readonly IGetCleanerContext _context;
            public CancelOrderCommandHandler(IGetCleanerContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
            {
                var order = await _context.Orders.AsTracking().Include(x => x.WorkingDays).FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted);

                if (order == null)
                    return Result.Failure("Order not exists!");

                order.ChangeStatusToCancel();
                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure("Save Error");

                return Result.Success();


            }
        }
    }
}
