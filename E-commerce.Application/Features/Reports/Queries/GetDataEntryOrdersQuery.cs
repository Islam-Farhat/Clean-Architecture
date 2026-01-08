using E_commerce.Application.Features.Orders.Dtos;
using E_commerce.Application.Features.Reports.Dtos;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Reports.Queries
{
    public class GetDataEntryOrdersQuery : IRequest<DataEntryOrdersDto>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int DataEntryId { get; set; }
        public OrderType? OrderType { get; set; } = null;
        public ShiftType? ShiftType { get; set; } = null;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        private class GetDataEntryOrdersQueryHandler : IRequestHandler<GetDataEntryOrdersQuery, DataEntryOrdersDto>
        {
            private readonly IGetCleanerContext _context;
            private readonly IConfiguration _configuration;
            public GetDataEntryOrdersQueryHandler(IGetCleanerContext context, IConfiguration configuration)
            {
                _context = context;
                _configuration = configuration;
            }
            public async Task<DataEntryOrdersDto> Handle(GetDataEntryOrdersQuery request, CancellationToken cancellationToken)
            {
                var fromDate = request.FromDate.Date;
                var toDate = request.ToDate.Date;
                var baseUrl = _configuration["GetCleaner:BaseUrl"];

                // Base query: WorkingDays assigned to this driver
                var query = _context.Orders
                                    .AsNoTracking()
                                    .Where(wd =>
                                        !wd.IsDeleted &&
                                        wd.UserId == request.DataEntryId &&
                                        wd.CreatedAt.Date >= fromDate &&
                                        wd.CreatedAt.Date <= toDate &&
                                        !wd.IsDeleted && wd.Status != OrderStatus.Cancelled);


                if (request.OrderType.HasValue)
                    query = query.Where(o => o.OrderType == request.OrderType.Value);

                if (request.ShiftType.HasValue)
                    query = query.Where(o => o.Shift == request.ShiftType.Value);


                var totalCount = await query.CountAsync();
                var totalPrice = await query.SumAsync(o => o.Price);

                var orders = await query.Select(x => new EntryOrderDto
                {
                    OrderId = x.Id,
                    CreatedAt = x.CreatedAt,
                    Comment = x.Comment,
                    Location = x.Location,
                    OrderCode = x.OrderCode,
                    Price = x.Price,
                    Shift = x.Shift,
                    OrderType = x.OrderType,
                    ApartmentNumber = x.ApartmentNumber,
                    PaymentType = x.PaymentType,
                    ApartmentImageUrl = string.IsNullOrWhiteSpace(x.ApartmentImageUrl)
                                                                        ? string.Empty
                                                                        : $"{baseUrl}ImageBank/Order/{x.ApartmentImageUrl}",

                })
                                        .Skip(request.Skip)
                                        .Take(request.Take)
                                        .ToListAsync(cancellationToken);

                return new DataEntryOrdersDto
                {
                    Orders = orders,
                    TotalCount = totalCount,
                    TotalPrice = totalPrice,
                };
            }
        }
    }
}
