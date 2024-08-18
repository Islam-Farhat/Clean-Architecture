using CSharpFunctionalExtensions;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Orders
{
    public class CreateOrderCommand : IRequest<Result<int>>
    {
        public string orderNumber { get; set; }
        public List<CreateItemDto> Items { get; set; }
        public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<int>>
        {
            private readonly IEcommerceContext _context;

            public CreateOrderCommandHandler(IEcommerceContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
            {
                var items = request.Items.Select(dto =>
               Item.Create(dto.Quantity, dto.Currency, dto.Amount, dto.ProductId)).ToList();

                var order = Order.Create(request.orderNumber, DateTime.Now, items);

                _context.Order.Add(order);

                var result = await _context.SaveChangesAsyncWithResult();
                if (result.IsSuccess)
                    return order.Id;

                return Result.Failure<int>("Can't Save");
            }
        }
    }
}
