using E_commerce.Application.Mapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            #region Fluent Validation

            // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
             services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            #endregion

            #region Inject Service

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddAutoMapper(typeof(EcommerceProfile));

            #endregion

            #region Mediator

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));


            #endregion

            return services;
        }
    }
}
