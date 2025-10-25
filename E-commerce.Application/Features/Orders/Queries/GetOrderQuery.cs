using E_commerce.Application.Features.Orders.Dtos;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Orders.Queries
{
    public class GetOrderQuery : IRequest<List<GetOrdersDto>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string SearchParam { get; set; } = string.Empty;
        public DateTime? WorkingDay { get; set; }

        private class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, List<GetOrdersDto>>
        {
            private readonly IGetCleanerContext _context;
            private readonly IConfiguration _configuration;

            public GetOrderQueryHandler(IGetCleanerContext context, IConfiguration configuration)
            {
                _context = context;
                _configuration = configuration;
            }
            public async Task<List<GetOrdersDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
            {
                var baseUrl = _configuration["GetCleaner:BaseUrl"];

                var orderQuery = _context.WorkingDays.Where(x=>x.WorkingDate.Date >= DateTime.UtcNow.Date).OrderBy(x=>x.WorkingDate).AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchParam))
                    orderQuery = orderQuery.Where(x => x.Order.ApartmentNumber.Contains(request.SearchParam) ||
                    x.Order.Housemaid.Name.Contains(request.SearchParam));

                if (request.WorkingDay != null || request.WorkingDay != default)
                {
                    orderQuery = orderQuery.Where(x => x.WorkingDate.Date == request.WorkingDay.Value.Date);
                }

                var orders = await orderQuery.Select(x => new GetOrdersDto
                                             {
                                                 Id = x.Id,
                                                 ApartmentNumber = x.Order.ApartmentNumber,
                                                 ImagePath = $"{baseUrl}ImageBank/Order/{x.Order.ApartmentImageUrl}",
                                                 OrderType = x.Order.OrderType,
                                                 Shift = x.Order.Shift,
                                                 DriverId = x.DriverId,
                                                 HousemaidName = x.Order.Housemaid.Name,
                                                 IsAssigned = x.DriverId != null ? true : false,
                                                 WorkingDay = x.WorkingDate.Date                                                
                                             })
                                             .Skip(request.Skip)
                                             .Take(request.Take)
                                             .ToListAsync();

                return orders;
            }
        }
    }
}
