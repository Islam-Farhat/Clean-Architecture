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
            // Validate inputs
            if (!Enum.IsDefined(typeof(OrderType), request.OrderType))
            {
                return new List<GetAvailableHousemaidDto>();
            }

            if (request.WorkingDays == null || !request.WorkingDays.Any())
            {
                return new List<GetAvailableHousemaidDto>();
            }

            // Step 1: Expand working days based on OrderType (same logic as order creation)
            var expandedDates = ExpandWorkingDays(request.WorkingDays, request.OrderType);

            if (!expandedDates.Any())
            {
                return new List<GetAvailableHousemaidDto>();
            }

            // Step 2: Get all housemaids
            var allHousemaids = await _context.Housemaids
                .Select(h => new { h.Id, h.Name })
                .ToListAsync(cancellationToken);

            if (!allHousemaids.Any())
            {
                return new List<GetAvailableHousemaidDto>();
            }

            // Step 3: Get all existing orders for this shift, with their working dates
            var bookedDatesByHousemaid = await _context.Orders
                .Where(o => o.OrderType != OrderType.Permanent || o.Shift == null || o.Shift == request.Shift)
                .Select(o => new
                {
                    o.HousemaidId,
                    Dates = o.WorkingDays.Select(wd => wd.WorkingDate.Date)
                })
                .ToListAsync(cancellationToken);

            // Step 4: Find housemaids who are FREE on ALL expanded dates
            var availableHousemaidIds = allHousemaids
                .Where(h => !bookedDatesByHousemaid.Any(b =>
                    b.HousemaidId == h.Id &&
                    b.Dates.Any(d => expandedDates.Contains(d))
                ))
                .Select(h => h.Id)
                .ToList();

            // Step 5: Return DTOs
            return allHousemaids
                .Where(h => availableHousemaidIds.Contains(h.Id))
                .Select(h => new GetAvailableHousemaidDto
                {
                    Id = h.Id,
                    Name = h.Name
                })
                .ToList();
        }

        private List<DateTime> ExpandWorkingDays(List<DateTime> inputDays, OrderType orderType)
        {
            var now = DateTime.UtcNow.Date;
            var endOfYear = new DateTime(now.Year, 12, 31);
            var result = new List<DateTime>();

            var uniqueInputDays = inputDays
                .Select(d => d.Date)
                .Distinct()
                .Where(d => d >= now)
                .ToList();

            if (!uniqueInputDays.Any()) return result;

            foreach (var day in uniqueInputDays)
            {
                if (orderType == OrderType.Weekly)
                {
                    var current = day;
                    while (current <= endOfYear)
                    {
                        result.Add(current);
                        current = current.AddDays(7);
                    }
                }
                else if (orderType == OrderType.Monthly)
                {
                    var current = day;
                    while (current <= endOfYear)
                    {
                        result.Add(current);
                        // Preserve day of month, but handle month overflow
                        try
                        {
                            current = current.AddMonths(1);
                        }
                        catch
                        {
                            // If day doesn't exist in next month (e.g., Jan 31 → Feb), skip or clamp
                            break;
                        }
                    }
                }
                else if (orderType == OrderType.Hourly || orderType == OrderType.Permanent)
                {
                    result.Add(day);
                }
            }

            return result.Distinct().OrderBy(d => d).ToList();
        }
    }

}