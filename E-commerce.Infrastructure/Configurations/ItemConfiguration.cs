using E_commerce.Domian;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.Price, moneyBuilder =>
            {
                moneyBuilder.Property(c => c.Currency);
                moneyBuilder.Property(a => a.Amount);
            });

            builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId);
        }
    }
}
