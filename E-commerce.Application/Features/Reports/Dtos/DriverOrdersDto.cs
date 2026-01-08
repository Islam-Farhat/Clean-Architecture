using E_commerce.Application.Features.Orders.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Reports.Dtos
{
    public class DriverOrdersDto
    {
        public List<GetOrdersDto> Orders { get; set; } = new();
        public int CompletedOrders { get; set; }
        public int ActiveOrders { get; set; }
        public int NewOrders { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
    }
}
