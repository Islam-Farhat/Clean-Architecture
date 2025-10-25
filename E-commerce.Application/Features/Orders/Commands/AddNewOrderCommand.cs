using CSharpFunctionalExtensions;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
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
        public int HousemaidId { get; set; }
        public string? Comment { get; set; }
        public string ApartmentImageBase64 { get; set; }
        public string ApartmentNumber { get; set; }
        public OrderType OrderType { get; set; }
        public ShiftType Shift { get; set; }
        public PaymentType PaymentType { get; set; }
        public List<DateTime> WorkingDays { get; set; } = new();

        public class AddNewOrderCommandHandler : IRequestHandler<AddNewOrderCommand, Result<int>>
        {
            private readonly IMediaService _mediaService;
            private readonly IGetCleanerContext _context;

            public AddNewOrderCommandHandler(IMediaService mediaService, IGetCleanerContext context)
            {
                _mediaService = mediaService;
                _context = context;
            }

            public async Task<Result<int>> Handle(AddNewOrderCommand request, CancellationToken cancellationToken)
            {
                if (!Enum.IsDefined(typeof(OrderType), request.OrderType))
                    return Result.Failure<int>("Invalid OrderType.");

                var fileName = await _mediaService.UploadImage(request.ApartmentImageBase64, "ImageBank/Order");
                if (string.IsNullOrWhiteSpace(fileName.Value))
                    return Result.Failure<int>("Upload Image Failed");

                var order = Order.Instance(
                    request.HousemaidId,
                    fileName.Value,
                    request.ApartmentNumber,
                    request.OrderType,
                    request.Shift,
                    request.PaymentType,
                    request.Comment
                );

                if (order.IsFailure)
                    return Result.Failure<int>(order.Error);

                var workingDays = new List<DateTime>();

                if (request.OrderType == OrderType.Weekly)
                {
                    // Repeat selected days every week until the end of the year
                    var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);
                    foreach (var day in request.WorkingDays.Distinct()) // Ensure unique input days
                    {
                        if (day < DateTime.UtcNow.Date)
                            continue; // Skip past dates

                        var currentDay = day;
                        while (currentDay <= endOfYear)
                        {
                            if (currentDay >= DateTime.UtcNow.Date)
                                workingDays.Add(currentDay);
                            currentDay = currentDay.AddDays(7); // Move to next week
                        }
                    }
                }
                else if (request.OrderType == OrderType.Monthly)
                {
                    // Repeat selected days every month until the end of the year
                    var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);
                    foreach (var day in request.WorkingDays.Distinct()) // Ensure unique input days
                    {
                        if (day < DateTime.UtcNow.Date)
                            continue; // Skip past dates

                        var currentDay = day;
                        while (currentDay <= endOfYear)
                        {
                            if (currentDay >= DateTime.UtcNow.Date)
                                workingDays.Add(currentDay);
                            currentDay = currentDay.AddMonths(1); // Move to next month
                        }
                    }
                }
                else if (request.OrderType == OrderType.Hourly)
                {
                    // For one-time orders, just use the provided working days
                    workingDays = request.WorkingDays.Where(d => d >= DateTime.UtcNow.Date).Distinct().ToList();
                }

                if (!workingDays.Any() && request.OrderType != OrderType.Permanent)
                    return Result.Failure<int>("No valid working days provided.");

                // Check for existing orders with same HousemaidId, Shift, and overlapping WorkingDays
                var existingWorkingDaysOrders = await _context.Orders
                    .Where(o => o.HousemaidId == request.HousemaidId && o.Shift == request.Shift)
                    .Select(x => x.WorkingDays).FirstOrDefaultAsync();

                if (existingWorkingDaysOrders != null)
                {
                    if (workingDays.Any(d => existingWorkingDaysOrders.Select(x => x.WorkingDate.Date).Contains(d.Date)))
                    {
                        return Result.Failure<int>("An order already exists for this housemaid, shift, and overlapping working days.");
                    }
                }


                // Ensure unique working days before adding to order
                var addWorkingDaysResult = order.Value.AddWorkingDays(workingDays.Distinct().ToList());
                if (addWorkingDaysResult.IsFailure && request.OrderType != OrderType.Permanent)
                    return Result.Failure<int>(addWorkingDaysResult.Error);

                // Save the order
                await _context.Orders.AddAsync(order.Value, cancellationToken);
                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure<int>("Save Error");

                return Result.Success(order.Value.Id);

            }
        }
    }
}