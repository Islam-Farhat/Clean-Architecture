using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.Entities
{
    public interface IGetCleanerContext
    {
        public DbSet<Housemaid> Housemaids { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<WorkingDay> WorkingDays { get; set; }
        Task<Result> SaveChangesAsyncWithResult();
    }
}
