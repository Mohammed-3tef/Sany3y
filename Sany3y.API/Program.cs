
using Sany3y.Infrastructure.Extensions;
using Sany3y.Infrastructure.Services;

namespace Sany3y.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Application Services
            builder.Services.AddApplicationServices();

            // Infrastructure Service
            builder.Services.AddInfrastructureServices(builder.Configuration);

            var app = builder.Build();
            app.UseCors("AllowAll");

            // Seed the database with initial data.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SeedService.SeedDatabase(services).Wait();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseAuthentication();

            app.MapControllers();
            app.Run();
        }
    }
}
