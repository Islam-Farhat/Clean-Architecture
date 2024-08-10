using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian
{
    public class Order
    {
        public readonly HashSet<Item> OrderItems = new();
        private Order()
        {
            
        }
        public int Id { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public DateTime OrderDate { get; private set; }

        public IReadOnlyList<Item> Items => OrderItems.ToList();
    }
}
