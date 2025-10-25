using CSharpFunctionalExtensions;
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
    }
}
