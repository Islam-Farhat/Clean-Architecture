using E_commerce.Domian.DomainEventHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.DominEvents.Order
{
    public class OrderCreatedDomainEvent : PostEvent
    {
        public int OrderId { get; set; }
    }
}
