using CSharpFunctionalExtensions;
using E_commerce.Application.Interfaces;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Orders.Commands
{
    public class AddNewOrderCommand : IRequest<Result<int>>
    {
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
        public class AddNewOrderCommandHandler : IRequestHandler<AddNewOrderCommand, Result<int>>
        {
            private readonly IMediaService _mediaService;
            private readonly IGetCleanerContext _context;
            private readonly ISessionUserService _sessionUser;
            private readonly UserManager<ApplicationUser> _userManager;
            public AddNewOrderCommandHandler(IMediaService mediaService, IGetCleanerContext context, ISessionUserService sessionUser, UserManager<ApplicationUser> userManager)
            {
                _mediaService = mediaService;
                _context = context;
                _sessionUser = sessionUser;
                _userManager = userManager;
            }

            public async Task<Result<int>> Handle(AddNewOrderCommand request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByIdAsync(_sessionUser?.UserId.ToString());
                if (user == null)
                    return Result.Failure<int>("User not found.");

                var isAdmin = await _userManager.IsInRoleAsync(user, RoleSystem.Admin.ToString());
                var isDataEntry = await _userManager.IsInRoleAsync(user, RoleSystem.DataEntry.ToString());

                if (!isAdmin && !isDataEntry)
                    return Result.Failure<int>("User is not a admin or dataEntiry.");

                if (!Enum.IsDefined(typeof(OrderType), request.OrderType))
                    return Result.Failure<int>("Invalid OrderType.");


                if (!request.HousemaidIds.Any())
                    return Result.Failure<int>("At least one housemaid must be selected.");

                request.Shift = request.OrderType == OrderType.Permanent ? null : request.Shift;

                var order = Order.Instance(
                    request.HousemaidIds.Distinct().ToList(),
                    request.ApartmentNumber,
                    request.OrderType,
                    request.Shift,
                    request.PaymentType,
                    request.Price,
                    _sessionUser.UserId,
                    isAdmin ? CreatedSource.Admin : CreatedSource.DataEntry,
                    request.Comment,
                    request.Location
                );

                if (!string.IsNullOrWhiteSpace(request.ApartmentImageBase64))
                {
                    var fileName = await _mediaService.UploadImage(request.ApartmentImageBase64, "ImageBank/Order");
                    if (string.IsNullOrWhiteSpace(fileName.Value))
                        return Result.Failure<int>("Upload Image Failed");

                    order.Value.UpdateApartmentImage(fileName.Value);
                }


                if (order.IsFailure)
                    return Result.Failure<int>(order.Error);

                var workingDays = GenerateWorkingDays(request.OrderType, request.WorkingDays);

                if (!workingDays.Any())
                    return Result.Failure<int>("No valid working days provided.");

                // Check conflict for EACH housemaid with the SAME shift
                foreach (var housemaidId in request.HousemaidIds.Distinct())
                {
                    var hasConflict = await _context.Orders
                        .AnyAsync(o =>
                            !o.IsDeleted &&
                            o.Status != OrderStatus.Cancelled &&
                            o.Shift == request.Shift &&
                            o.OrderHousemaids.Any(oh => oh.HousemaidId == housemaidId) &&
                            o.WorkingDays.Any(wd => !wd.IsDeleted && workingDays.Select(d => d.Date).Contains(wd.WorkingDate.Date)),
                            cancellationToken);

                    if (hasConflict)
                        return Result.Failure<int>($"Housemaid ID {housemaidId} has a scheduling conflict on selected dates.");
                }

                // Ensure unique working days before adding to order
                var addWorkingDaysResult = order.Value.AddWorkingDays(workingDays.Distinct().ToList());
                if (addWorkingDaysResult.IsFailure)
                    return Result.Failure<int>(addWorkingDaysResult.Error);

                // Save the order
                await _context.Orders.AddAsync(order.Value, cancellationToken);
                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure<int>("Save Error");

                return Result.Success(order.Value.Id);

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