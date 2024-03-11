using E_commerce.Infrastructure;
using E_commerce.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using E_commerce.API.Middlewares;

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
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
