using Microsoft.AspNetCore.Authentication;
using Sany3y.Extensions;
using Sany3y.Hubs;
using Sany3y.Infrastructure.Services;

namespace Sany3y
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSignalR();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Application Services
            builder.Services.AddApplicationServices();            

            // Infrastructure Service
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Configure Google Authentication
            builder.Services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

                options.ClaimActions.MapJsonKey("picture", "picture", "url");
            });

            var app = builder.Build();

            // Seed the database with initial data.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SeedService.SeedDatabase(services).Wait();
            }

            // Configure middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<UserStatusHub>("/userStatusHub");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}
