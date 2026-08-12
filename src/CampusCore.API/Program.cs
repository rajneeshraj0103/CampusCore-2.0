
using CampusCore.API.Middleware;
using CampusCore.Application.Services;
using CampusCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CampusCore.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<CampusCoreDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("CampusCoreDb")));

            builder.Services.AddScoped<IUserService, UserService>();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
