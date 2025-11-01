using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sany3y.Hubs;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
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

            // Register repositories
            builder.Services.AddScoped<UserRepository, UserRepository>();
            builder.Services.AddScoped<IRepository<Address>, AddressRepository>();
            builder.Services.AddScoped<IRepository<Category>, CategoryRepository>();
            builder.Services.AddScoped<IRepository<Message>, MessageRepository>();
            builder.Services.AddScoped<IRepository<ProfilePicture>, ProfilePictureRepository>();
            builder.Services.AddScoped<IRepository<Notification>, NotificationRepository>();
            builder.Services.AddScoped<IRepository<Rating>, RatingRepository>();
            builder.Services.AddScoped<IRepository<Sany3y.Infrastructure.Models.Task>, TaskRepository>();

            // Register services
            builder.Services.AddTransient<IEmailSender, EmailConfirm>();

            // Register DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("MainDB"));
            });

            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

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
