using CSharpFunctionalExtensions;
using E_commerce.Application.Features.Housemaid.Dtos;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Features.Housemaid.Queries
{
    public class GetAvailableHousemaidsForEditQuery : IRequest<List<GetAvailableHousemaidDto>>
    {
        public int OrderId { get; set; }
        public ShiftType? Shift { get; set; }
        public OrderType OrderType { get; set; }
        public List<DateTime> WorkingDays { get; set; } = new();
    }

    public class GetAvailableHousemaidsForEditQueryHandler : IRequestHandler<GetAvailableHousemaidsForEditQuery, List<GetAvailableHousemaidDto>>
    {
        private readonly IGetCleanerContext _context;

        public GetAvailableHousemaidsForEditQueryHandler(IGetCleanerContext context)
        {
            _context = context;
        }

        public async Task<List<GetAvailableHousemaidDto>> Handle(GetAvailableHousemaidsForEditQuery request, CancellationToken cancellationToken)
        {
            // Basic validation
            if (!Enum.IsDefined(typeof(OrderType), request.OrderType))
                return new List<GetAvailableHousemaidDto>();

            if (request.WorkingDays == null || !request.WorkingDays.Any())
                return new List<GetAvailableHousemaidDto>();

            if (request.OrderType != OrderType.Permanent && !request.Shift.HasValue)
                return new List<GetAvailableHousemaidDto>();

            // Expand the new desired dates (same logic as create)
            var requestedDates = ExpandWorkingDays(request.WorkingDays, request.OrderType);
            if (!requestedDates.Any())
                return new List<GetAvailableHousemaidDto>();

            var requestedDateSet = requestedDates.Select(d => d.Date).ToHashSet();

            // Load all ACTIVE orders EXCEPT the one we're editing
            var activeOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted
                            && o.Status != OrderStatus.Cancelled
                            && o.Id != request.OrderId)
                .Select(o => new
                {
                    o.OrderType,
                    o.Shift,
                    HousemaidIds = o.OrderHousemaids.Select(oh => oh.HousemaidId).ToList(),
                    WorkingDates = o.WorkingDays
                        .Where(wd => !wd.IsDeleted)
                        .Select(wd => wd.WorkingDate.Date)
                        .ToList()
                })
                .Where(o => o.HousemaidIds.Any())
                .ToListAsync(cancellationToken);

            // Build occupied slots per housemaid (same as before)
            var occupiedByHousemaid = new Dictionary<int, HashSet<(DateTime Date, ShiftType? Shift)>>();

            foreach (var order in activeOrders)
            {
                foreach (var housemaidId in order.HousemaidIds)
                {
                    if (!occupiedByHousemaid.TryGetValue(housemaidId, out var occupiedSet))
                    {
                        occupiedSet = new HashSet<(DateTime, ShiftType?)>();
                        occupiedByHousemaid[housemaidId] = occupiedSet;
                    }

                    if (order.OrderType == OrderType.Permanent)
                    {
                        foreach (var date in order.WorkingDates)
                        {
                            occupiedSet.Add((date, ShiftType.Shift1));
                            occupiedSet.Add((date, ShiftType.Shift2));
                        }
                    }
                    else if (order.Shift.HasValue)
                    {
                        foreach (var date in order.WorkingDates)
                        {
                            occupiedSet.Add((date, order.Shift.Value));
                        }
                    }
                }
            }

            // Get all housemaids
            var allHousemaids = await _context.Housemaids
                // .Where(h => h.IsActive) // uncomment if needed
                .Select(h => new { h.Id, h.Name })
                .ToListAsync(cancellationToken);

            var availableHousemaids = new List<GetAvailableHousemaidDto>();

            foreach (var housemaid in allHousemaids)
            {
                if (!occupiedByHousemaid.TryGetValue(housemaid.Id, out var occupiedSet))
                {
                    // No conflicts at all
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
                    // Permanent requires housemaid to have zero bookings (from other orders)
                    if (occupiedSet.Any())
                        isAvailable = false;
                }
                else
                {
                    var requestedShift = request.Shift.Value;
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
            var result = new HashSet<DateTime>();

            var uniqueInputDays = inputDays
                .Select(d => d.Date)
                .Distinct()
                .Where(d => d >= now)
                .ToList();

            if (!uniqueInputDays.Any() && orderType != OrderType.Permanent)
                return new List<DateTime>();

            foreach (var baseDay in uniqueInputDays)
            {
                switch (orderType)
                {
                    case OrderType.Weekly:
                        var currentWeekly = baseDay;
                        while (currentWeekly <= endOfYear)
                        {
                            result.Add(currentWeekly);
                            currentWeekly = currentWeekly.AddDays(7);
                        }
                        break;

                    case OrderType.Monthly:
                        var currentMonthly = baseDay;
                        while (currentMonthly <= endOfYear)
                        {
                            result.Add(currentMonthly);
                            try
                            {
                                currentMonthly = currentMonthly.AddMonths(1);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                break;
                            }
                        }
                        break;

                    case OrderType.Hourly:
                        result.Add(baseDay);
                        break;

                    case OrderType.Permanent:
                        for (var d = now; d <= endOfYear; d = d.AddDays(1))
                        {
                            result.Add(d);
                        }
                        return result.OrderBy(d => d).ToList();
                }
            }

            return result.OrderBy(d => d).ToList();
        }
    }
}