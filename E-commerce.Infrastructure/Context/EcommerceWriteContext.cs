using E_commerce.Domian;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Context
{
    public class EcommerceWriteContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public EcommerceWriteContext(DbContextOptions<EcommerceWriteContext> options)
      : base(options)
        {
        }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Item> Items { get; set; }
    }
}
