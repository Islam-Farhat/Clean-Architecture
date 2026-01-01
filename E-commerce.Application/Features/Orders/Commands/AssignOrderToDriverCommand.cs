using CSharpFunctionalExtensions;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Orders.Commands
{
    public class AssignOrderToDriverCommand : IRequest<Result>
    {
        public int WorkingOrderId { get; set; }
        public int DriverId { get; set; }
        public class AssignOrderToDriverCommandHandler : IRequestHandler<AssignOrderToDriverCommand, Result>
        {
            private readonly IGetCleanerContext _context;
            private readonly UserManager<ApplicationUser> _userManager;

            public AssignOrderToDriverCommandHandler(IGetCleanerContext context, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Result> Handle(AssignOrderToDriverCommand request, CancellationToken cancellationToken)
            {
                var driver = await _userManager.FindByIdAsync(request.DriverId.ToString());
                if (driver == null)
                {
                    return Result.Failure("Driver not found.");
                }

                var isUserInRoleDriver = await _userManager.IsInRoleAsync(driver, RoleSystem.Driver.ToString());
                if (!isUserInRoleDriver)
                {
                    return Result.Failure("User is not in the Driver role.");
                }

                // Fetch the working day
                var workingDateOrder = await _context.WorkingDays
                    .AsTracking()
                    .Include(wd => wd.Order) // Include Order to access OrderType
                    .FirstOrDefaultAsync(x => x.Id == request.WorkingOrderId, cancellationToken);

                if (workingDateOrder == null)
                {
                    return Result.Failure("Working day order does not exist.");
                }

                // Use Saudi Arabia time zone (AST) for date validation
                var saudiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                var currentDateInAst = TimeZoneInfo.ConvertTime(DateTime.UtcNow, saudiTimeZone).Date;

                // Validate that the working day is today or in the future (skip for Permanent orders)
                if (workingDateOrder.Order?.OrderType != OrderType.Permanent &&
                    workingDateOrder.WorkingDate.Date < currentDateInAst)
                {
                    return Result.Failure("Cannot assign order for a past working day.");
                }

                // Assign the driver to the working day
                var assignResult = workingDateOrder.AssignOrderToDriver(request.DriverId);
                if (assignResult.IsFailure)
                {
                    return Result.Failure(assignResult.Error);
                }

                // Save changes
                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                {
                    return Result.Failure("Failed to save changes.");
                }

                return Result.Success();

            }
        }
    }
}
