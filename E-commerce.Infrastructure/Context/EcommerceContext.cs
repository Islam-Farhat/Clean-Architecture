using E_commerce.Domian;
using E_commerce.Domian.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Context
{
    public class EcommerceContext : DbContext, IEcommerceContext
    {
        public EcommerceContext(DbContextOptions<EcommerceContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(EcommerceContext).Assembly);
        }

        public Task<int> SaveChangesAsync()
        {
           return  base.SaveChangesAsync();
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}
