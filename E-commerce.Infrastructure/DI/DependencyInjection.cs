using E_commerce.Application.Interfaces;
using E_commerce.Application.Services.Interfaces;
using E_commerce.Domian;
using E_commerce.Domian.Entities;
using E_commerce.Infrastructure.Context;
using E_commerce.Infrastructure.Seeding;
using E_commerce.Infrastructure.Services;
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

            var connectionstring = configuration.GetConnectionString("ECORead");
            services.AddDbContext<GetCleanerContext>(option => option.UseSqlServer(connectionstring));

            services.AddScoped<IGetCleanerContext, GetCleanerContext>();
            services.AddScoped<IRoleSeedService, RoleSeedService>();
            services.AddScoped<IMediaService, MediaService>();

            #endregion

            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<ISessionUserService, SessionUserService>();

            return services;
        }
    }
}
