using CSharpFunctionalExtensions;
using E_commerce.Domian;
using E_commerce.Domian.DomainEventHelper;
using E_commerce.Domian.Entities;
using MediatR;
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
        private readonly IPublisher _publisher;
        public EcommerceContext(DbContextOptions<EcommerceContext> options, IPublisher publisher)
        : base(options)
        {
            _publisher = publisher;
        }
        public DbSet<Product> Product { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Order> Order { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(EcommerceContext).Assembly);
        }

        public async Task<Result> SaveChangesAsyncWithResult()
        {
            try
            {
                var result = await base.SaveChangesAsync();
                await FirePostDomainEvents();
                return Result.Success();
            }
            catch (Exception exp)
            {
                return Result.Failure(exp.Message);
            }
        }

        private async Task FirePostDomainEvents()
        {
            var domainEvents = ChangeTracker.Entries<Domian.DomainEventHelper.Entity>()
                .Select(x => x.Entity)
                .Where(x => x.Events.Any(X => X.GetType().IsSubclassOf(typeof(PostEvent))))
                .SelectMany(x => x.Events)
                .ToList();

            foreach (var domianEntity in domainEvents)
            {
                await _publisher.Publish(domianEntity);
            }
        }

    }
}
