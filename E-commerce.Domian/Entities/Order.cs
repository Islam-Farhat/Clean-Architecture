using E_commerce.Domian.DomainEventHelper;
using E_commerce.Domian.DominEvents.Order;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian
{
    public class Order : Entity
    {
        public readonly HashSet<Item> _OrderItems = new();
        private Order()
        {

        }
        public int Id { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public DateTime OrderDate { get; private set; }

        public IReadOnlyList<Item> Items => _OrderItems.ToList();

        private Order(string orderNumber, DateTime orderDate)
        {
            OrderNumber = orderNumber ?? throw new ArgumentNullException(nameof(orderNumber));
            OrderDate = orderDate;
        }

        public static Order Create(string orderNumber, DateTime orderDate, List<Item> items)
        {
            if (items == null || !items.Any()) throw new ArgumentException("Order must have at least one item.");

            var order = new Order(orderNumber, orderDate);

            foreach (var item in items)
            {
                order.AddItem(item);
            }

            order.AddEvent(new OrderCreatedDomainEvent() { OrderId = order.Id });
            return order;
        }

        private void AddItem(Item item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _OrderItems.Add(item);
        }
    }
}
