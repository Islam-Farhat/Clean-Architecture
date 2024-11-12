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
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("TA_Products");

            builder.HasKey(x => x.Id);

            builder.HasOne<Category>()
                   .WithMany(x => x.Products)
                   .HasForeignKey(x => x.CategoryId);

            builder.OwnsOne(x => x.Price, moneyBuilder =>
            {
                moneyBuilder.Property(c => c.Currency);
                moneyBuilder.Property(a => a.Amount);
            });
        }
    }
}
