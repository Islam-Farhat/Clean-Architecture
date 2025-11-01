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
using System.Security.Claims;
using E_commerce.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace E_commerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
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
            .AddEntityFrameworkStores<GetCleanerContext>();
            #endregion

            builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));//Mapping values to class option pattern

            #region Setup Bearer
            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateIssuer = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:key"])),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1)//buffer token to become valid after expiration to 1 minute
                };
            });
            #endregion
            builder.Services.AddHttpContextAccessor();
            var app = builder.Build();
          //  app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
               Path.Combine(builder.Environment.ContentRootPath, "ImageBank")),
                RequestPath = "/ImageBank"
            });

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            app.MapControllers();

            // Apply migrations and seed roles during startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<GetCleanerContext>();
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    // Apply migrations
                    logger.LogInformation("Applying database migrations...");
                    context.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully.");

                    // Seed roles
                    var roleSeedService = services.GetRequiredService<IRoleSeedService>();
                    logger.LogInformation("Seeding roles...");
                    await roleSeedService.SeedRolesAsync();
                    logger.LogInformation("Roles seeded successfully.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while applying migrations or seeding roles.");
                    throw; // Optionally rethrow or handle differently based on your requirements
                }
            }

            app.Run();
        }
    }
}
