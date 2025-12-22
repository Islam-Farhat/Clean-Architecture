using CSharpFunctionalExtensions;
using E_commerce.Domian.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.Entities
{
    public class WorkingDay
    {
        private WorkingDay() { }
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime WorkingDate { get; set; }
        public bool IsDeleted { get; set; }
        public int? DriverId { get; set; }
        public DeliveringStatus DeliveringStatus { get; set; } = DeliveringStatus.New;
        public PaymentType? PaymentType { get; set; }
        public string? Comments { get; set; }
        public decimal? Amount { get; set; } = 0;
        public string? PaymentImage { get; set; }
        public Order Order { get; set; }
        public ApplicationUser Driver { get; set; }

        public static Result<WorkingDay> Instance(DateTime workingDate)
        {
            if (workingDate == default || workingDate.Date < DateTime.UtcNow.Date)
                return Result.Failure<WorkingDay>("WorkingDay must be a valid future or current date.");

            var workingDayEntity = new WorkingDay
            {
                WorkingDate = workingDate
            };

            return Result.Success(workingDayEntity);
        }

        public Result AssignOrderToDriver(int driverId)
        {
            this.DriverId = driverId; 
            return Result.Success();
        }

        public void Delete()
        {
            IsDeleted = true;
        }
        
        public void ChangeStatusToCancel()
        {
            this.DeliveringStatus = DeliveringStatus.Cancelled;
        }

        
        public void ChangeStatusToDeliveredToHome()
        {
            this.DeliveringStatus = DeliveringStatus.DeliveredToHome;
        }

        public void ChangeStatusToEndOfShift(PaymentType? paymentType, decimal? amount ,string? comments)
        {
            this.DeliveringStatus = DeliveringStatus.EndOfShift;
            this.Amount = amount;
            this.PaymentImage = comments;
        }

        public void UpdatePaymentImage(string imageUrl)
        {
            this.PaymentImage = imageUrl;
        }
    }
}
