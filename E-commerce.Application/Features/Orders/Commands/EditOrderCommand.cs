using CSharpFunctionalExtensions;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Entities;
using E_commerce.Domian;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Features.Orders.Commands
{
    public class EditOrderCommand : IRequest<Result>
    {
        public int OrderId { get; set; }
        public List<int> HousemaidIds { get; set; } = new();
        public string? Comment { get; set; } = string.Empty;
        public string? ApartmentImageBase64 { get; set; } = string.Empty;
        public string ApartmentNumber { get; set; }
        public string? Location { get; set; } = string.Empty;
        public OrderType OrderType { get; set; }
        public ShiftType? Shift { get; set; } = null;
        public PaymentType PaymentType { get; set; }
        public List<DateTime> WorkingDays { get; set; } = new();
        public decimal Price { get; set; } = 0;

        public class EditOrderCommandHandler : IRequestHandler<EditOrderCommand, Result>
        {
            private readonly IMediaService _mediaService;
            private readonly IGetCleanerContext _context;
            private readonly ISessionUserService _sessionUser;
            private readonly UserManager<ApplicationUser> _userManager;
            public EditOrderCommandHandler(IMediaService mediaService, IGetCleanerContext context, ISessionUserService sessionUser, UserManager<ApplicationUser> userManager)
            {
                _mediaService = mediaService;
                _context = context;
                _sessionUser = sessionUser;
                _userManager = userManager;
            }
            public async Task<Result> Handle(EditOrderCommand request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByIdAsync(_sessionUser?.UserId.ToString());
                if (user == null)
                    return Result.Failure("User not found.");

                var isAdmin = await _userManager.IsInRoleAsync(user, RoleSystem.Admin.ToString());
                var isDataEntry = await _userManager.IsInRoleAsync(user, RoleSystem.DataEntry.ToString());

                if (!isAdmin && !isDataEntry)
                    return Result.Failure("Unauthorized: Only Admin or DataEntry can edit orders.");

                var order = await _context.Orders
                    .AsTracking()
                    .Include(o => o.WorkingDays.Where(x => x.WorkingDate >= DateTime.UtcNow))
                    .Include(o => o.OrderHousemaids)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken);

                if (order == null)
                    return Result.Failure("Order not found or already deleted.");

                if (!Enum.IsDefined(typeof(OrderType), request.OrderType))
                    return Result.Failure("Invalid OrderType.");

                if (request.OrderType == OrderType.Permanent)
                    request.Shift = null;

                if (!string.IsNullOrWhiteSpace(request.ApartmentImageBase64))
                {
                    var uploadResult = await _mediaService.UploadImage(request.ApartmentImageBase64, "ImageBank/Order");
                    if (uploadResult.IsFailure || string.IsNullOrWhiteSpace(uploadResult.Value))
                        return Result.Failure("Failed to upload apartment image.");

                    order.UpdateApartmentImage(uploadResult.Value);
                }


                var newWorkingDays = GenerateWorkingDays(request.OrderType, request.WorkingDays);

                if (!newWorkingDays.Any())
                    return Result.Failure("No valid future working days provided.");

                foreach (var housemaidId in request.HousemaidIds.Distinct())
                {
                    var hasConflict = await _context.Orders
                        .Where(o =>
                            o.Id != order.Id &&
                            !o.IsDeleted &&
                            o.Status != OrderStatus.Cancelled &&
                            o.Shift == request.Shift &&
                            o.OrderHousemaids.Any(oh => oh.HousemaidId == housemaidId))
                        .SelectMany(o => o.WorkingDays)
                        .AnyAsync(wd =>
                            !wd.IsDeleted &&
                            newWorkingDays.Select(d => d.Date).Contains(wd.WorkingDate.Date),
                            cancellationToken);

                    if (hasConflict)
                        return Result.Failure($"Housemaid ID {housemaidId} has a scheduling conflict on the selected dates.");
                }

                var updateResult = order.Update(
                    housemaidIds: request.HousemaidIds.Distinct().ToList(),
                    apartmentNumber: request.ApartmentNumber,
                    comment: request.Comment,
                    location: request.Location,
                    orderType: request.OrderType,
                    shift: request.Shift,
                    paymentType: request.PaymentType,
                    price: request.Price
                );

                if (updateResult.IsFailure)
                    return Result.Failure(updateResult.Error);

                foreach (var wd in order.WorkingDays.ToList())
                    wd.Delete();

                var addWorkingDaysResult = order.AddWorkingDays(newWorkingDays.Distinct().ToList());
                if (addWorkingDaysResult.IsFailure)
                    return Result.Failure(addWorkingDaysResult.Error);

                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure("Failed to save changes.");

                return Result.Success();
            }


            private List<DateTime> GenerateWorkingDays(OrderType orderType, List<DateTime> selectedWorkingDays)
            {
                var workingDays = new List<DateTime>();

                // Use only future or today dates
                var validSelectedDays = selectedWorkingDays
                    .Where(d => d.Date >= DateTime.UtcNow.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                if (!validSelectedDays.Any())
                    return workingDays; // empty

                var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);

                switch (orderType)
                {
                    case OrderType.Weekly:
                        foreach (var startDay in validSelectedDays)
                        {
                            var current = startDay;
                            while (current <= endOfYear)
                            {
                                workingDays.Add(current);
                                current = current.AddDays(7);
                            }
                        }
                        break;

                    case OrderType.Monthly:
                        foreach (var startDay in validSelectedDays)
                        {
                            var current = startDay;
                            while (current <= endOfYear)
                            {
                                workingDays.Add(current);
                                current = current.AddMonths(1);
                            }
                        }
                        break;

                    case OrderType.Hourly:
                        // One-time: just use the selected days
                        workingDays.AddRange(validSelectedDays);
                        break;

                    case OrderType.Permanent:
                        // Every day from today until end of year
                        var startDate = DateTime.UtcNow.Date;
                        for (var day = startDate; day <= endOfYear; day = day.AddDays(1))
                        {
                            workingDays.Add(day);
                        }
                        break;

                    default:
                        break;
                }

                return workingDays.Distinct().OrderBy(d => d).ToList();
            }

        }
    }
}
