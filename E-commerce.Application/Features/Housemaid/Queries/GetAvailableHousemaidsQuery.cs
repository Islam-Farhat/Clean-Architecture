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
        public ShiftType Shift { get; set; }
        public List<DateTime> WorkingDays { get; set; }

        public class GetAvailableHousemaidsQueryHandler : IRequestHandler<GetAvailableHousemaidsQuery, List<GetAvailableHousemaidDto>>
        {
            private readonly IGetCleanerContext _context;

            public GetAvailableHousemaidsQueryHandler(IGetCleanerContext context)
            {
                _context = context;
            }

            public async Task<List<GetAvailableHousemaidDto>> Handle(GetAvailableHousemaidsQuery request, CancellationToken cancellationToken)
            {
                // Validate input
                if (!Enum.IsDefined(typeof(ShiftType), request.Shift))
                {
                    return new List<GetAvailableHousemaidDto>(); // Return empty list for invalid shift
                }

                if (request.WorkingDays == null || !request.WorkingDays.Any())
                {
                    return new List<GetAvailableHousemaidDto>(); // Return empty list if no working days provided
                }

                // Normalize working days to dates only and remove duplicates
                var requestedDates = request.WorkingDays
                    .Select(d => d.Date)
                    .Distinct()
                    .Where(d => d >= DateTime.UtcNow.Date) // Only future or current dates
                    .ToList();

                if (!requestedDates.Any())
                {
                    return new List<GetAvailableHousemaidDto>(); // Return empty list if no valid dates
                }

                // Get all housemaids
                var allHousemaids = await _context.Housemaids
                    .Select(h => new { h.Id, h.Name }) // Assuming Housemaid entity has Id and Name
                    .ToListAsync(cancellationToken);

                // Get all orders with their working days for the specified shift
                var ordersWithWorkingDays = await _context.Orders
                    .Where(o => o.Shift == request.Shift)
                    .Select(o => new { o.HousemaidId, WorkingDays = o.WorkingDays.Select(wd => wd.WorkingDate.Date) })
                    .ToListAsync(cancellationToken);

                // Find housemaids with conflicting schedules
                var bookedHousemaidIds = ordersWithWorkingDays
                    .Where(o => o.WorkingDays.Any(wd => requestedDates.Contains(wd)))
                    .Select(o => o.HousemaidId)
                    .Distinct()
                    .ToList();

                // Filter available housemaids (those not in bookedHousemaidIds)
                var availableHousemaids = allHousemaids
                    .Where(h => !bookedHousemaidIds.Contains(h.Id))
                    .Select(h => new GetAvailableHousemaidDto
                    {
                        Id = h.Id,
                        Name = h.Name
                    })
                    .ToList();

                return availableHousemaids;
            }
        }
    }
}