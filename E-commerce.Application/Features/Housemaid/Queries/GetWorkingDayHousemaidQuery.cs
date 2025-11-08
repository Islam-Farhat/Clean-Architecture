using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Housemaid.Queries
{
    public class GetWorkingDayHousemaidQuery : IRequest<List<DateTime>>
    {
        public int HousemaidId { get; set; }
        public ShiftType Shift { get; set; }

        private class GetWorkingDayHousemaidQueryHandler : IRequestHandler<GetWorkingDayHousemaidQuery, List<DateTime>>
        {
            private readonly IGetCleanerContext _context;

            public GetWorkingDayHousemaidQueryHandler(IGetCleanerContext context)
            {
                _context = context;
            }
            public async Task<List<DateTime>> Handle(GetWorkingDayHousemaidQuery request, CancellationToken cancellationToken)
            {

                var workingDays =await _context.Orders
                                        .Where(x => x.HousemaidId == request.HousemaidId && x.Shift == request.Shift)
                                        .SelectMany(x => x.WorkingDays.Select(w => w.WorkingDate))
                                        .ToListAsync();

                return workingDays ?? new List<DateTime>();
            }
        }
    }
}
