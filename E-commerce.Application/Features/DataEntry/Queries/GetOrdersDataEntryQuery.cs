using E_commerce.Application.Features.Orders.Dtos;
using E_commerce.Application.Interfaces;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.DataEntry.Queries
{
    public class GetOrdersDataEntryQuery : IRequest<List<GetOrdersDto>>
    {
        public int UserId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public string SearchParam { get; set; } = string.Empty;
        public DateTime? WorkingDay { get; set; }
        public ShiftType? Shift { get; set; } = null;
        public OrderType? OrderType { get; set; } = null;

        public class GetOrdersDataEntryQueryHandler : IRequestHandler<GetOrdersDataEntryQuery, List<GetOrdersDto>>
        {
            private readonly IGetCleanerContext _context;
            private readonly IConfiguration _configuration;
            private readonly ISessionUserService _sessionUser;
            private readonly UserManager<ApplicationUser> _userManager;

            public GetOrdersDataEntryQueryHandler(
                IGetCleanerContext context,
                IConfiguration configuration,
                ISessionUserService sessionUser,
                UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _configuration = configuration;
                _sessionUser = sessionUser;
                _userManager = userManager;
            }
            public async Task<List<GetOrdersDto>> Handle(GetOrdersDataEntryQuery request, CancellationToken cancellationToken)
            {
                var baseUrl = _configuration["GetCleaner:BaseUrl"];

                var user = await _userManager.FindByIdAsync(_sessionUser?.UserId.ToString());
                if (user == null)
                    return new List<GetOrdersDto>();


                var orderQuery = _context.WorkingDays
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.WorkingDate)
                    .AsQueryable();


                if (!string.IsNullOrWhiteSpace(request.SearchParam))
                {
                    var search = request.SearchParam.Trim();
                    orderQuery = orderQuery.Where(x =>
                        x.Order.ApartmentNumber.Contains(search) ||
                        x.Order.OrderHousemaids.Any(o=>o.Housemaid.Name.Contains(search)) ||
                        x.Order.OrderCode.Contains(search));
                }

                if (request.WorkingDay.HasValue)
                {
                    orderQuery = orderQuery.Where(x => x.WorkingDate.Date == request.WorkingDay.Value.Date);
                }

                if (request.Shift.HasValue)
                {
                    orderQuery = orderQuery.Where(x => x.Order.Shift == request.Shift.Value);
                }

                if (request.OrderType.HasValue)
                {
                    orderQuery = orderQuery.Where(x => x.Order.OrderType == request.OrderType.Value);
                }

                var orders = await orderQuery
                    .Where(x => x.Order.UserId == request.UserId)
                    .Select(x => new GetOrdersDto
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

                return orders;
            }
        }
    }
}
