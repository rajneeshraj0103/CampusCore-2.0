using CampusCore.API.Middleware;
using CampusCore.Application.Interfaces;
using CampusCore.Application.Services;
using CampusCore.Infrastructure.Data;
using CampusCore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampusCore.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddNewtonsoftJson();

            builder.Services.AddDbContext<CampusCoreDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("CampusCoreDb")));

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<GlobalExceptionMiddleware>();

            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
