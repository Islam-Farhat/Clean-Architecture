using E_commerce.Domian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Orders
{
    public class CreateItemDto
    {
        public int Quantity { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public int ProductId { get; set; }
    }
}
