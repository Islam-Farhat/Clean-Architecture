using CSharpFunctionalExtensions;
using E_commerce.Application.Features.Housemaid.Dtos;
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

namespace E_commerce.Application.Features.Housemaid.Queries
{
    public class GetAvailableHousemaidsQuery : IRequest<List<GetAvailableHousemaidDto>>
    {
        public ShiftType? Shift { get; set; }
        public OrderType OrderType { get; set; }
        public List<DateTime> WorkingDays { get; set; } = new();
    }

    public class GetAvailableHousemaidsQueryHandler : IRequestHandler<GetAvailableHousemaidsQuery, List<GetAvailableHousemaidDto>>
    {
        private readonly IGetCleanerContext _context;

        public GetAvailableHousemaidsQueryHandler(IGetCleanerContext context)
        {
            _context = context;
        }

        public async Task<List<GetAvailableHousemaidDto>> Handle(GetAvailableHousemaidsQuery request, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(OrderType), request.OrderType))
                return new List<GetAvailableHousemaidDto>();

            if (request.WorkingDays == null || !request.WorkingDays.Any())
                return new List<GetAvailableHousemaidDto>();

            // For Permanent orders, shift is null — but they block everything
            // For others, shift is required
            if (request.OrderType != OrderType.Permanent && !request.Shift.HasValue)
                return new List<GetAvailableHousemaidDto>();

            var requestedDates = ExpandWorkingDays(request.WorkingDays, request.OrderType);
            if (!requestedDates.Any())
                return new List<GetAvailableHousemaidDto>();

            var requestedDateSet = requestedDates.Select(d => d.Date).ToHashSet();

            // Load all active orders with their working days and shift
            var activeOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.Status != OrderStatus.Cancelled)
                .Select(o => new
                {
                    o.HousemaidId,
                    o.OrderType,
                    o.Shift, // null for Permanent
                    WorkingDates = o.WorkingDays
                        .Where(wd => !wd.IsDeleted)
                        .Select(wd => wd.WorkingDate.Date)
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            // Build occupied (date, shift) pairs per housemaid
            var occupiedByHousemaid = new Dictionary<int, HashSet<(DateTime Date, ShiftType? Shift)>>();

            foreach (var order in activeOrders)
            {
                if (!occupiedByHousemaid.TryGetValue(order.HousemaidId, out var occupiedSet))
                {
                    occupiedSet = new HashSet<(DateTime, ShiftType?)>();
                    occupiedByHousemaid[order.HousemaidId] = occupiedSet;
                }

                if (order.OrderType == OrderType.Permanent)
                {
                    // Permanent blocks BOTH shifts on all its working days
                    foreach (var date in order.WorkingDates)
                    {
                        occupiedSet.Add((date, ShiftType.Shift1));
                        occupiedSet.Add((date, ShiftType.Shisft2));
                    }
                }
                else if (order.Shift.HasValue)
                {
                    // Non-permanent: blocks only its own shift on its dates
                    foreach (var date in order.WorkingDates)
                    {
                        occupiedSet.Add((date, order.Shift.Value));
                    }
                }
            }

            // Get all housemaids
            var allHousemaids = await _context.Housemaids
                //.Where(h => h.IsActive) // optional: only active ones
                .Select(h => new { h.Id, h.Name })
                .ToListAsync(cancellationToken);

            // Check availability
            var availableHousemaids = new List<GetAvailableHousemaidDto>();

            foreach (var housemaid in allHousemaids)
            {
                if (!occupiedByHousemaid.TryGetValue(housemaid.Id, out var occupiedSet))
                {
                    // No bookings at all → fully available
                    availableHousemaids.Add(new GetAvailableHousemaidDto
                    {
                        Id = housemaid.Id,
                        Name = housemaid.Name
                    });
                    continue;
                }

                bool isAvailable = true;

                if (request.OrderType == OrderType.Permanent)
                {
                    // Requesting permanent → must have no bookings at all
                    if (occupiedSet.Any())
                    {
                        isAvailable = false;
                    }
                }
                else // Hourly, Weekly, Monthly → check specific shift
                {
                    var requestedShift = request?.Shift;

                    foreach (var date in requestedDateSet)
                    {
                        if (occupiedSet.Contains((date, requestedShift)))
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                }

                if (isAvailable)
                {
                    availableHousemaids.Add(new GetAvailableHousemaidDto
                    {
                        Id = housemaid.Id,
                        Name = housemaid.Name
                    });
                }
            }

            return availableHousemaids;
        }

        private List<DateTime> ExpandWorkingDays(List<DateTime> inputDays, OrderType orderType)
        {
            var now = DateTime.UtcNow.Date;
            var endOfYear = new DateTime(now.Year, 12, 31);
            var result = new HashSet<DateTime>(); // Use HashSet to avoid duplicates

            var uniqueInputDays = inputDays
                .Select(d => d.Date)
                .Distinct()
                .Where(d => d >= now)
                .ToList();

            if (!uniqueInputDays.Any()) return new List<DateTime>();

            foreach (var baseDay in uniqueInputDays)
            {
                if (orderType == OrderType.Weekly)
                {
                    var current = baseDay;
                    while (current <= endOfYear)
                    {
                        result.Add(current);
                        current = current.AddDays(7);
                    }
                }
                else if (orderType == OrderType.Monthly)
                {
                    var current = baseDay;
                    while (current <= endOfYear)
                    {
                        result.Add(current);
                        try
                        {
                            current = current.AddMonths(1);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // e.g., Jan 31 → Feb doesn't exist → stop this chain
                            break;
                        }
                    }
                }
                else if (orderType == OrderType.Hourly)
                {
                    result.Add(baseDay);
                }
                else if (orderType == OrderType.Permanent)
                {
                    // Permanent uses all days from now to end of year (same as AddNewOrderCommand)
                    for (var d = now; d <= endOfYear; d = d.AddDays(1))
                    {
                        result.Add(d);
                    }
                    // No need to process further input days — permanent is all days
                    break;
                }
            }

            return result.OrderBy(d => d).ToList();
        }
    }
}