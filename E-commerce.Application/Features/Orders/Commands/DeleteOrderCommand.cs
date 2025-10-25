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

namespace E_commerce.Application.Features.Orders.Commands
{
    public class DeleteOrderCommand : IRequest<Result>
    {
        public int Id { get; set; }
        private class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result>
        {
            private readonly IGetCleanerContext _context;
            private readonly IMediaService _mediaService;

            public DeleteOrderCommandHandler(IGetCleanerContext context, IMediaService mediaService)
            {
                _context = context;
                _mediaService = mediaService;
            }
            public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
            {
                var order = await _context.Orders.Include(x => x.WorkingDays).FirstOrDefaultAsync(x => x.Id == request.Id);

                if (order == null)
                    return Result.Failure("Order not exists!");

                _context.Orders.Remove(order);
                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure("Save Error");


                if (!string.IsNullOrWhiteSpace(order.ApartmentImageUrl))
                {
                    var deleteResult = await _mediaService.DeleteImage($"ImageBank\\Order\\{order.ApartmentImageUrl}");
                    if (deleteResult.IsFailure)
                    {
                        // Log the failure but proceed with deletion (optional)
                        System.Diagnostics.Debug.WriteLine($"Failed to delete image: {deleteResult.Error}");
                    }
                }

                return Result.Success();
            }
        }
    }
}
