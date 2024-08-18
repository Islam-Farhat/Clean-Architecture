using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian
{
    public class Item
    {
        public int Id { get; private set; }
        public int Quantity { get; private set; }
        public Money Price { get; private set; }
        public int ProductId { get; private set; }
        public int OrderId { get; private set; }

        public static Item Create(int quantity,string currency,decimal amount,int productId)
        {
            var money = new Money(currency, amount);

            var item = new Item()
            {
                Quantity = quantity,
                Price = money,
                ProductId = productId
            };

            return item;
        }
    }
}
