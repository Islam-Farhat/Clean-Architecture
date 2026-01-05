using E_commerce.Domian.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Orders.Dtos
{
    public class GetOrdersDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ApartmentNumber { get; set; }
        public string? ImagePath { get; set; }
        public OrderType OrderType { get; set; }
        public ShiftType? Shift { get; set; }
        public DateTime WorkingDay { get; set; }
        public bool IsAssigned { get; set; }
        public int? DriverId { get; set; }
        public DeliveringStatus DeliveringStatus { get; set; } = DeliveringStatus.New;
        public string? Comments { get; set; }
        public decimal? Amount { get; set; } = 0;
        public string? PaymentImage { get; set; }
        public string? Location { get; set; }
        public string OrderCode { get; set; }
        public string? DriverName { get; set; }
        public List<HousemaidDto> Housemaids { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime? StartShiftDate { get; set; }
        public DateTime? EndShiftDate { get; set; }
    }


    public class HousemaidDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
