using CSharpFunctionalExtensions;
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
        public string? ApartmentImageUrl { get; set; }
        public string ApartmentNumber { get; set; }
        public OrderType OrderType { get; set; }
        public ShiftType? Shift { get; set; }
        public PaymentType PaymentType { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Active;
        public DateTime CreatedAt { get; set; }
        public CreatedSource CreateBy  { get; set; } = CreatedSource.Admin;
        public decimal Price { get; set; } = 0;
        public string? Location { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public int? UserId { get; set; }
        public Housemaid Housemaid { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<WorkingDay> WorkingDays { get; set; }

        public static Result<Order> Instance(
            int housemaidId,
            string apartmentNumber,
            OrderType orderType,
            PaymentType paymentType,
            decimal price,
            int? userId,
            CreatedSource createdSource,
            ShiftType? shift = null,
            string? comment = null,
            string? location = null
            )
        {
            if (housemaidId <= 0)
                return Result.Failure<Order>("HousemaidId must be greater than zero.");
            if (string.IsNullOrWhiteSpace(apartmentNumber))
                return Result.Failure<Order>("ApartmentNumber is required.");
            if (orderType == default)
                return Result.Failure<Order>("OrderType is required.");
            if (paymentType == default)
                return Result.Failure<Order>("PaymentType is required.");            
            if (orderType != OrderType.Permanent && shift == null )
                return Result.Failure<Order>("Shift is required.");

            var order = new Order
            {
                HousemaidId = housemaidId,
                ApartmentNumber = apartmentNumber,
                OrderType = orderType,
                Shift = orderType == OrderType.Permanent ? null : shift,
                PaymentType = paymentType,
                CreatedAt = DateTime.UtcNow,
                Comment = comment,
                Price = price,
                CreateBy = createdSource,
                UserId = userId,
                Location = location,
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
        public void UpdateApartmentImage(string apartmentImageUrl)
        {
            this.ApartmentImageUrl = apartmentImageUrl;
        }

        public void Delete()
        {
            this.IsDeleted = true;
            foreach (var workingDay in this.WorkingDays)
            {
                workingDay.Delete();
            }
        }
    }
}
