using E_commerce.Application.Features.Orders.Dtos;
using E_commerce.Domian.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Reports.Dtos
{
    public class DataEntryOrdersDto
    {
        public List<EntryOrderDto> Orders { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public int TotalCount { get; set; }
    }

    public class EntryOrderDto
    {
        public int OrderId { get; set; }
        public string? Comment { get; set; }
        public string? ApartmentImageUrl { get; set; }
        public string ApartmentNumber { get; set; }
        public OrderType OrderType { get; set; }
        public ShiftType? Shift { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Price { get; set; } = 0;
        public string? Location { get; set; } = string.Empty;
        public string OrderCode { get; set; }
    }
}
