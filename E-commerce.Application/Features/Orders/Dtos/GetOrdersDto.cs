﻿using E_commerce.Domian.Enums;
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
        public string ApartmentNumber { get; set; }
        public string ImagePath { get; set; }
        public OrderType OrderType { get; set; }
        public ShiftType Shift { get; set; }
        public DateTime WorkingDay { get; set; }
        public string HousemaidName { get; set; }
        public bool IsAssigned { get; set; }
        public int? DriverId { get; set; }
    }
}
