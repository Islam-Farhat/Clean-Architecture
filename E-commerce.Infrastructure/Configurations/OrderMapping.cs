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
    public class OrderMapping : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.WorkingDays).WithOne(x => x.Order);
            builder.Property(x => x.OrderCode).HasDefaultValue("0000000000");
            builder.HasIndex(x => x.OrderCode).IsUnique();

        }
    }
}
