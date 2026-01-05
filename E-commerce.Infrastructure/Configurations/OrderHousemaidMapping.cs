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
    public class OrderHousemaidMapping : IEntityTypeConfiguration<OrderHousemaid>
    {
        public void Configure(EntityTypeBuilder<OrderHousemaid> builder)
        {
            // Composite primary key
            builder.HasKey(oh => new { oh.OrderId, oh.HousemaidId });

            // Foreign key to Order
            builder.HasOne(oh => oh.Order)
                   .WithMany(o => o.OrderHousemaids)
                   .HasForeignKey(oh => oh.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Foreign key to Housemaid
            builder.HasOne(oh => oh.Housemaid)
                   .WithMany()
                   .HasForeignKey(oh => oh.HousemaidId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}