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
        //public int HousemaidId { get; private set; }
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
        //public Housemaid Housemaid { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<OrderHousemaid> OrderHousemaids { get; private set; } = new List<OrderHousemaid>();
        public ICollection<WorkingDay> WorkingDays { get; set; }


        public static Result<Order> Instance(
                List<int> housemaidIds,
                string apartmentNumber,
                OrderType orderType,
                ShiftType? shift,
                PaymentType paymentType,
                decimal price,
                int? userId,
                CreatedSource createdSource,
                string? comment = null,
                string? location = null)
        {
            if (!housemaidIds?.Any() ?? true)
                return Result.Failure<Order>("At least one housemaid is required.");

            if (housemaidIds.Any(id => id <= 0))
                return Result.Failure<Order>("Invalid HousemaidId.");

            if (string.IsNullOrWhiteSpace(apartmentNumber))
                return Result.Failure<Order>("ApartmentNumber is required.");

            if (orderType == default)
                return Result.Failure<Order>("OrderType is required.");

            if (paymentType == default)
                return Result.Failure<Order>("PaymentType is required.");

            if (orderType != OrderType.Permanent && shift == null)
                return Result.Failure<Order>("Shift is required for non-permanent orders.");

            var order = new Order
            {
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
                OrderCode = GenerateOrderCode(),
                OrderHousemaids = housemaidIds.Select(id => OrderHousemaid.Create(0, id)).ToList()
            };

            return Result.Success(order);
        }


        public Result Update(
            List<int> housemaidIds,
            string apartmentNumber,
            string? comment,
            string? location,
            OrderType orderType,
            ShiftType? shift,
            PaymentType paymentType,
            decimal price)
        {
            if (!housemaidIds?.Any() ?? true)
                return Result.Failure("At least one housemaid is required.");

            if (housemaidIds.Any(id => id <= 0))
                return Result.Failure("Invalid HousemaidId.");

            if (string.IsNullOrWhiteSpace(apartmentNumber))
                return Result.Failure("ApartmentNumber is required.");

            if (orderType != OrderType.Permanent && shift == null)
                return Result.Failure("Shift is required for non-permanent orders.");


            this.ApartmentNumber = apartmentNumber;
            this.Comment = comment;
            this.Location = location;
            this.OrderType = orderType;
            this.Shift = orderType == OrderType.Permanent ? null : shift;
            this.PaymentType = paymentType;
            this.Price = price;

            // Replace all housemaids
            this.OrderHousemaids.Clear();
            foreach (var id in housemaidIds.Distinct())
            {
                this.OrderHousemaids.Add(OrderHousemaid.Create(this.Id, id));
            }

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
