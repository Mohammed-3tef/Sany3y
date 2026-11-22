using Sany3y.API.Extensions;
using Sany3y.API.Hubs;
using Sany3y.Infrastructure.Services;
using System.Diagnostics;
using System.Text;

namespace Sany3y.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Stripe
            Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddSignalR();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            // Application Services
            builder.Services.AddApplicationServices();

            // Infrastructure Service
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // JWT Authentication Service
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerWithJwt();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // شغل FastAPI
            try
            {
                var pyDir = Path.Combine(Directory.GetCurrentDirectory(), "py");
                if (Directory.Exists(pyDir))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c python -m uvicorn app:app --host 0.0.0.0 --port 8000",
                        WorkingDirectory = pyDir,
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };
                    Process.Start(psi);
                }
            }
            catch
            {
                // Ignore if Python / uvicorn is already running or not installed
            }

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

            app.MapHub<UserStatusHub>("/userStatusHub");
            app.MapHub<ChatHub>("/chatHub");
            app.MapControllers();
            app.Run();
        }
    }
}