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
        public int Id { get; private set; }
        public int HousemaidId { get; private set; }
        public string? Comment { get; private set; }
        public string? ApartmentImageUrl { get; private set; }
        public string ApartmentNumber { get; private set; }
        public OrderType OrderType { get; private set; }
        public ShiftType? Shift { get; private set; }
        public PaymentType PaymentType { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Active;
        public DateTime CreatedAt { get; private set; }
        public CreatedSource CreateBy { get; private set; } = CreatedSource.Admin;
        public decimal Price { get; private set; } = 0;
        public string? Location { get; private set; } = string.Empty;
        public bool IsDeleted { get; private set; }
        public int? UserId { get; private set; }
        public string OrderCode { get; private set; }
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
            if (orderType != OrderType.Permanent && shift == null)
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
                OrderCode = GenerateOrderCode()
            };

            return Result.Success(order);
        }


        public Result Update(
                int housemaidId,
                string apartmentNumber,
                string? comment,
                string? location,
                OrderType orderType,
                ShiftType? shift,
                PaymentType paymentType,
                decimal price)
        {
            if (housemaidId <= 0)
                return Result.Failure("HousemaidId must be greater than zero.");

            if (string.IsNullOrWhiteSpace(apartmentNumber))
                return Result.Failure("ApartmentNumber is required.");

            if (!Enum.IsDefined(typeof(OrderType), orderType))
                return Result.Failure("Invalid OrderType.");

            if (!Enum.IsDefined(typeof(PaymentType), paymentType))
                return Result.Failure("Invalid PaymentType.");

            if (orderType != OrderType.Permanent && shift == null)
                return Result.Failure("Shift is required for non-permanent orders.");

            // Apply updates
            this.HousemaidId = housemaidId;
            this.ApartmentNumber = apartmentNumber;
            this.Comment = comment;
            this.Location = location;
            this.OrderType = orderType;
            this.Shift = orderType == OrderType.Permanent ? null : shift;
            this.PaymentType = paymentType;
            this.Price = price;

            return Result.Success();
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

        public void ChangeStatusToCancel()
        {
            this.Status = OrderStatus.Cancelled;

            foreach (var workingDay in this.WorkingDays)
            {
                workingDay.ChangeStatusToCancel();
            }
        }

        private static string GenerateOrderCode()
        {
            var random = new Random();

            long number = (long)(random.NextDouble() * 10_000_000_000L);
            return number.ToString("D10");
        }

    }
}
