using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Orders
{
    public class CreateOrderDto
    {
        public string orderNumber { get; set; }
        public List<CreateItemDto> Items { get; set; }
    }
}
