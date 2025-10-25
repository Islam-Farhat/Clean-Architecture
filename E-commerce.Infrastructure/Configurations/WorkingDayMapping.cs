using E_commerce.Domian.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Configurations
{
    internal class WorkingDayMapping : IEntityTypeConfiguration<WorkingDay>
    {
        public void Configure(EntityTypeBuilder<WorkingDay> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Driver).WithMany(x => x.Workdays);
        }
    }
}
