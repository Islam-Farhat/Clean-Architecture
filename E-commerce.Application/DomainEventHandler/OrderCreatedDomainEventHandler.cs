using E_commerce.Domian.DominEvents.Order;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.DomainEventHandler
{
    public class OrderCreatedDomainEventHandler
        : INotificationHandler<OrderCreatedDomainEvent>
    {
        public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Order {notification.OrderId} was created.");
        }
    }
}
