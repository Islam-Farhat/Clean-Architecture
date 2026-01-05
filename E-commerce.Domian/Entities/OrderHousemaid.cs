using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.Entities
{
    public class OrderHousemaid
    {
        public int OrderId { get; private set; }
        public int HousemaidId { get; private set; }

        public Order Order { get; set; } = null!;
        public Housemaid Housemaid { get; set; } = null!;

        private OrderHousemaid() { }

        public static OrderHousemaid Create(int orderId, int housemaidId)
        {
            return new OrderHousemaid
            {
                OrderId = orderId,
                HousemaidId = housemaidId
            };
        }
    }
}
