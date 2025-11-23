using Sany3y.API.Extensions;
using Sany3y.Extensions;
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

            // Application Services
            builder.Services.AddApplicationServices();

            // Infrastructure Service
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // JWT Authentication Service
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerWithJwt();

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
