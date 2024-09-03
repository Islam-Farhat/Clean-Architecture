using E_commerce.Infrastructure;
using E_commerce.Application;
using Microsoft.AspNetCore.Identity;
using E_commerce.API.Middlewares;
using E_commerce.Domian;
using E_commerce.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using E_commerce.Application.Helper;

namespace E_commerce.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            #region Injection in Infrastructure, Application ,and Presentation layer

            var configuration = builder.Configuration;
            builder.Services.AddInfrastructure(configuration);
            builder.Services.AddApplication();

            var presentationAssembly = typeof(Presentation.AssemblyReference).Assembly;
            builder.Services.AddControllers()
                .AddApplicationPart(presentationAssembly);

            #endregion

            #region Middlewares

            builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

            #endregion

            #region Inject identity (userManager)
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddEntityFrameworkStores<EcommerceContext>();
            #endregion

            #region Setup Bearer
            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:key"]))
                };
            });

            #endregion

            builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));//Mapping values to class

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
