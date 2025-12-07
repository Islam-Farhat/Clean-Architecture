using CSharpFunctionalExtensions;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Drivers.Commands
{
    public class DeleteDriverCommand : IRequest<Result>
    {
        public int UserId { get; set; }
    }

    public class DeleteDriverCommandHandler : IRequestHandler<DeleteDriverCommand, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGetCleanerContext _context;

        public DeleteDriverCommandHandler(
            UserManager<ApplicationUser> userManager,
            IGetCleanerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<Result> Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId.ToString()))
                return Result.Failure("Driver ID is required.");

            var driver = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (driver == null)
                return Result.Failure("Driver not found.");

            var isDriver = await _userManager.IsInRoleAsync(driver, RoleSystem.Driver.ToString());
            if (!isDriver)
                return Result.Failure("User is not a driver.");

            var hasOrders = await _context.WorkingDays
                .AnyAsync(o => o.DriverId == request.UserId, cancellationToken);

            if (hasOrders)
                return Result.Failure("Cannot delete driver because they have associated orders.");

            var result = await _userManager.DeleteAsync(driver);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to delete driver: {errors}");
            }

            return Result.Success();
        }
    }
}