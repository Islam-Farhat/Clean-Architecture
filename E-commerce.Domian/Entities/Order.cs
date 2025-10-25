﻿using CSharpFunctionalExtensions;
using E_commerce.Domian.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.Entities
{
    public class Order
    {
        private Order() { }
        public int Id { get; set; }
        public int HousemaidId { get; set; }
        public string? Comment { get; set; }
        public string ApartmentImageUrl { get; set; }
        public string ApartmentNumber { get; set; }
        public OrderType OrderType { get; set; }
        public ShiftType Shift { get; set; }
        public PaymentType PaymentType { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Active;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Housemaid Housemaid { get; set; }
        public ICollection<WorkingDay> WorkingDays { get; set; }

        public static Result<Order> Instance(
            int housemaidId,
            string apartmentImageUrl,
            string apartmentNumber,
            OrderType orderType,
            ShiftType shift,
            PaymentType paymentType,
            string? comment = null)
        {
            if (housemaidId <= 0)
                return Result.Failure<Order>("HousemaidId must be greater than zero.");
            if (string.IsNullOrWhiteSpace(apartmentImageUrl))
                return Result.Failure<Order>("ApartmentImageUrl is required.");
            if (string.IsNullOrWhiteSpace(apartmentNumber))
                return Result.Failure<Order>("ApartmentNumber is required.");
            if (orderType == default)
                return Result.Failure<Order>("OrderType is required.");
            if (shift == default)
                return Result.Failure<Order>("Shift is required.");
            if (paymentType == default)
                return Result.Failure<Order>("PaymentType is required.");



            var order = new Order
            {
                HousemaidId = housemaidId,
                ApartmentImageUrl = apartmentImageUrl,
                ApartmentNumber = apartmentNumber,
                OrderType = orderType,
                Shift = shift,
                PaymentType = paymentType,
                CreatedAt = DateTime.UtcNow,
                Comment = comment,
            };

            return Result.Success(order);
        }
        public Result AddWorkingDays(List<DateTime> workingdays)
        {
            if (!workingdays.Any())
                return Result.Failure<Order>("Working days are required.");

            this.WorkingDays = workingdays.Select(x => WorkingDay.Instance(x).Value).ToList();

            return Result.Success();
        }
    }
}
