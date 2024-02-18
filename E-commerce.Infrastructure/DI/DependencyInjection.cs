using E_commerce.Domian;
using E_commerce.Infrastructure.Context;
using E_commerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            #region Database

            var connectionstring = configuration.GetConnectionString("ECO");
            services.AddDbContext<EcommerceContext>(option => option.UseSqlServer(connectionstring));

            #endregion

            #region Inject Repos
            services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
            #endregion

            return services;
        }
    }
}
