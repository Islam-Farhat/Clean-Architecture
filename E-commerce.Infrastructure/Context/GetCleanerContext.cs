using CSharpFunctionalExtensions;
using E_commerce.Domian;
using E_commerce.Domian.DomainEventHelper;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Context
{
    public class GetCleanerContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>, IGetCleanerContext
    {
        private readonly IPublisher _publisher;
        public GetCleanerContext(DbContextOptions<GetCleanerContext> options, IPublisher publisher)
        : base(options)
        {
            _publisher = publisher;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity => { entity.ToTable(name: "Users"); });
            builder.Entity<ApplicationRole>(entity => { entity.ToTable(name: "Roles"); });
            builder.Entity<IdentityUserRole<int>>(entity => { entity.ToTable(name: "UserRoles"); });
            builder.Entity<IdentityUserClaim<int>>(entity => { entity.ToTable(name: "UserClaims"); });
            builder.Entity<IdentityUserLogin<int>>(entity => { entity.ToTable(name: "UserLogins"); });
            builder.Entity<IdentityRoleClaim<int>>(entity => { entity.ToTable(name: "RoleClaims"); });
            builder.Entity<IdentityUserToken<int>>(entity => { entity.ToTable(name: "UserTokens"); });

            builder.ApplyConfigurationsFromAssembly(typeof(GetCleanerContext).Assembly);
        }

        public DbSet<Housemaid> Housemaids { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<WorkingDay> WorkingDays { get; set; }



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
