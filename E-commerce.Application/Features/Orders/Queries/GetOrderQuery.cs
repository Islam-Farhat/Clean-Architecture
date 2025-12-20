using CSharpFunctionalExtensions;
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
            private readonly ISessionUserService _sessionUser;
            private readonly UserManager<ApplicationUser> _userManager;

            public GetOrderQueryHandler(IGetCleanerContext context, IConfiguration configuration, ISessionUserService sessionUser, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _configuration = configuration;
                _sessionUser = sessionUser;
                _userManager = userManager;
            }
            public async Task<List<GetOrdersDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
            {
                var baseUrl = _configuration["GetCleaner:BaseUrl"];

                var orderQuery = _context.WorkingDays.Where(x => !x.IsDeleted).OrderBy(x => x.WorkingDate).AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchParam))
                    orderQuery = orderQuery.Where(x => x.Order.ApartmentNumber.Contains(request.SearchParam) ||
                    x.Order.Housemaid.Name.Contains(request.SearchParam));

                if (request.WorkingDay != null || request.WorkingDay != default)
                {
                    orderQuery = orderQuery.Where(x => x.WorkingDate.Date == request.WorkingDay.Value.Date);
                }
                var user = await _userManager.FindByIdAsync(_sessionUser?.UserId.ToString());
                if (user == null)
                    return new List<GetOrdersDto>();
                var isAdmin = await _userManager.IsInRoleAsync(user, RoleSystem.Admin.ToString());
                var orders = await orderQuery.Where(x => isAdmin || x.Order.UserId == _sessionUser.UserId)
                                              .Select(x => new GetOrdersDto
                                              {
                                                  Id = x.Id,
                                                  ApartmentNumber = x.Order.ApartmentNumber,
                                                  ImagePath = string.IsNullOrWhiteSpace(x.Order.ApartmentImageUrl) ? string.Empty : $"{baseUrl}ImageBank/Order/{x.Order.ApartmentImageUrl}",
                                                  OrderType = x.Order.OrderType,
                                                  Shift = x.Order.Shift,
                                                  DriverId = x.DriverId,
                                                  HousemaidName = x.Order.Housemaid.Name,
                                                  IsAssigned = x.DriverId != null ? true : false,
                                                  WorkingDay = x.WorkingDate.Date,
                                                  Amount = x.Amount,
                                                  Location = x.Order.Location,
                                                  Comments = x.Comments,
                                                  DeliveringStatus = x.DeliveringStatus,
                                                  PaymentImage = string.IsNullOrWhiteSpace(x.PaymentImage) ? string.Empty : $"{baseUrl}ImageBank/WorkingDay/{x.PaymentImage}",
                                              })
                                              .Skip(request.Skip)
                                              .Take(request.Take)
                                              .ToListAsync();

                return orders;
            }
        }
    }
}
