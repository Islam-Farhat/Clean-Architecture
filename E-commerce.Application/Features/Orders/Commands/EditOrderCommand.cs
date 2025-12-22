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
        public int HousemaidId { get; set; }
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
                // 1. Get current user and check role
                var user = await _userManager.FindByIdAsync(_sessionUser?.UserId.ToString());
                if (user == null)
                    return Result.Failure("User not found.");

                var isAdmin = await _userManager.IsInRoleAsync(user, RoleSystem.Admin.ToString());
                var isDataEntry = await _userManager.IsInRoleAsync(user, RoleSystem.DataEntry.ToString());

                if (!isAdmin && !isDataEntry)
                    return Result.Failure("Unauthorized: Only Admin or DataEntry can edit orders.");

                // 2. Find the order with working days
                var order = await _context.Orders
                    .AsTracking()
                    .Include(o => o.WorkingDays)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken);

                if (order == null)
                    return Result.Failure("Order not found or already deleted.");

                // 3. Validate OrderType
                if (!Enum.IsDefined(typeof(OrderType), request.OrderType))
                    return Result.Failure("Invalid OrderType.");

                // Permanent orders have no shift
                if (request.OrderType == OrderType.Permanent)
                    request.Shift = null;

                // 4. Handle new apartment image (if provided)
                if (!string.IsNullOrWhiteSpace(request.ApartmentImageBase64))
                {
                    var uploadResult = await _mediaService.UploadImage(request.ApartmentImageBase64, "ImageBank/Order");
                    if (uploadResult.IsFailure || string.IsNullOrWhiteSpace(uploadResult.Value))
                        return Result.Failure("Failed to upload apartment image.");

                    order.UpdateApartmentImage(uploadResult.Value);
                }

                // 5. Generate new working days based on OrderType (same logic as AddNewOrder)
                var newWorkingDays = new List<DateTime>();

                if (request.OrderType == OrderType.Weekly)
                {
                    var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);
                    foreach (var day in request.WorkingDays.Distinct())
                    {
                        if (day.Date < DateTime.UtcNow.Date) continue;
                        var current = day.Date;
                        while (current <= endOfYear)
                        {
                            newWorkingDays.Add(current);
                            current = current.AddDays(7);
                        }
                    }
                }
                else if (request.OrderType == OrderType.Monthly)
                {
                    var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);
                    foreach (var day in request.WorkingDays.Distinct())
                    {
                        if (day.Date < DateTime.UtcNow.Date) continue;
                        var current = day.Date;
                        while (current <= endOfYear)
                        {
                            newWorkingDays.Add(current);
                            current = current.AddMonths(1);
                        }
                    }
                }
                else if (request.OrderType == OrderType.Hourly)
                {
                    newWorkingDays = request.WorkingDays
                        .Where(d => d.Date >= DateTime.UtcNow.Date)
                        .Distinct()
                        .ToList();
                }
                else if (request.OrderType == OrderType.Permanent)
                {
                    var startDate = DateTime.UtcNow.Date;
                    var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);
                    newWorkingDays.AddRange(
                        Enumerable.Range(0, (endOfYear - startDate).Days + 1)
                                  .Select(i => startDate.AddDays(i))
                    );
                }

                if (!newWorkingDays.Any())
                    return Result.Failure("No valid future working days provided.");

                // 6. Check for conflicts with OTHER orders (same housemaid + shift + overlapping dates)
                var conflictingOrders = await _context.Orders
                    .Where(o => o.Id != order.Id &&
                                o.HousemaidId == request.HousemaidId &&
                                o.Shift == request.Shift &&
                                !o.IsDeleted &&
                                o.Status != OrderStatus.Cancelled)
                    .Include(o => o.WorkingDays)
                    .ToListAsync(cancellationToken);

                foreach (var conflictingOrder in conflictingOrders)
                {
                    var conflictingDates = conflictingOrder.WorkingDays
                        .Where(wd => !wd.IsDeleted)
                        .Select(wd => wd.WorkingDate.Date);

                    if (newWorkingDays.Any(d => conflictingDates.Contains(d.Date)))
                        return Result.Failure("Cannot edit: Housemaid has overlapping working days in another active order.");
                }

                // 7. Update order properties
                var updateResult = order.Update( housemaidId: request.HousemaidId,
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

                // 8. Update working days: delete old, add new
                foreach (var wd in order.WorkingDays.ToList())
                {
                    wd.Delete(); // Soft delete old ones
                }

                var addWorkingDaysResult = order.AddWorkingDays(newWorkingDays.Distinct().ToList());
                if (addWorkingDaysResult.IsFailure)
                    return Result.Failure(addWorkingDaysResult.Error);

                // 9. Save changes
                var saveResult = await _context.SaveChangesAsyncWithResult();
                if (saveResult.IsFailure)
                    return Result.Failure("Failed to save changes.");

                return Result.Success();
            }
        }
    }
}
