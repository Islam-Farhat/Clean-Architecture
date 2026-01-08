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
    public class GetDriverOrdersQuery : IRequest<DriverOrdersDto>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int DriverId { get; set; }
        public OrderType? OrderType { get; set; } = null;
        public ShiftType? ShiftType { get; set; } = null;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        private class GetDriverOrdersQueryHandler : IRequestHandler<GetDriverOrdersQuery, DriverOrdersDto>
        {
            private readonly IGetCleanerContext _context;
            private readonly IConfiguration _configuration;
            public GetDriverOrdersQueryHandler(IGetCleanerContext context, IConfiguration configuration)
            {
                _context = context;
                _configuration = configuration;
            }
            public async Task<DriverOrdersDto> Handle(GetDriverOrdersQuery request, CancellationToken cancellationToken)
            {
                var fromDate = request.FromDate.Date;
                var toDate = request.ToDate.Date;
                var baseUrl = _configuration["GetCleaner:BaseUrl"];

                // Base query: WorkingDays assigned to this driver
                var query = _context.WorkingDays
                                    .AsNoTracking()
                                    .Where(wd =>
                                        !wd.IsDeleted &&
                                        wd.DriverId == request.DriverId &&
                                        wd.WorkingDate.Date >= fromDate &&
                                        wd.WorkingDate.Date <= toDate &&
                                        !wd.Order.IsDeleted && wd.Order.Status != OrderStatus.Cancelled);


                if (request.OrderType.HasValue)
                    query = query.Where(o => o.Order.OrderType == request.OrderType.Value);

                if (request.ShiftType.HasValue)
                    query = query.Where(o => o.Order.Shift == request.ShiftType.Value);


                var totalCount = await query.CountAsync();
                var totalPrice = await query.SumAsync(o => o.Order.Price);
                var totalAmount = await query.SumAsync(o => o.Amount.HasValue ? o.Amount.Value : 0);
                var newOrders = await query.CountAsync(o => o.DeliveringStatus == DeliveringStatus.New || o.DeliveringStatus == DeliveringStatus.AssignedToDriver);
                var activeOrders = await query.CountAsync(o => o.DeliveringStatus == DeliveringStatus.DeliveredToHome);
                var completedOrders = await query.CountAsync(o => o.DeliveringStatus == DeliveringStatus.Finished);

                var orders = await query.Select(x => new GetOrdersDto
                                        {
                                            Id = x.Id,
                                            OrderId = x.OrderId,
                                            ApartmentNumber = x.Order.ApartmentNumber,
                                            ImagePath = string.IsNullOrWhiteSpace(x.Order.ApartmentImageUrl)
                                                                        ? string.Empty
                                                                        : $"{baseUrl}ImageBank/Order/{x.Order.ApartmentImageUrl}",
                                            OrderType = x.Order.OrderType,
                                            Shift = x.Order.Shift,
                                            DriverId = x.DriverId,
                                            Housemaids = x.Order.OrderHousemaids.Select(oh => new HousemaidDto { Id = oh.HousemaidId, Name = oh.Housemaid.Name }).ToList(),
                                            IsAssigned = x.DriverId != null,
                                            WorkingDay = x.WorkingDate.Date,
                                            Amount = x.Amount,
                                            Location = x.Order.Location,
                                            Comments = x.Comments,
                                            DeliveringStatus = x.DeliveringStatus,
                                            PaymentImage = string.IsNullOrWhiteSpace(x.PaymentImage)
                                                                        ? string.Empty
                                                                        : $"{baseUrl}ImageBank/WorkingDay/{x.PaymentImage}",
                                            OrderCode = x.Order.OrderCode,
                                            PaymentType = x.Order.PaymentType,
                                            DriverName = x.Driver != null ? x.Driver.UserName : string.Empty,
                                            EndShiftDate = x.EndShiftDate,
                                            StartShiftDate = x.StartShiftDate,
                                            Price = x.Order.Price

                                        })
                                        .Skip(request.Skip)
                                        .Take(request.Take)
                                        .ToListAsync(cancellationToken);

                return new DriverOrdersDto
                {
                    Orders = orders,
                    ActiveOrders = activeOrders,
                    CompletedOrders = completedOrders,
                    NewOrders = newOrders,
                    TotalAmount = totalAmount,
                    TotalCount = totalCount,
                    TotalPrice = totalPrice,
                };
            }
        }
    }
}
