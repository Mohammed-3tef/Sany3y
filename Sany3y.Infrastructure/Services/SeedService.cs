using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.Infrastructure.Services
{
    public class SeedService
    {
        private static List<string> roles = new List<string> { "Admin", "Tasker", "User" };
        private static IRepository<Address> _addressRepository;

        public SeedService(IRepository<Address> addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public static async System.Threading.Tasks.Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                // Ensure the database is ready
                logger.LogInformation("Ensuring the database is created.");
                await context.Database.EnsureCreatedAsync();

                // Add roles
                logger.LogInformation("Seeding roles.");
                foreach (var role in roles)
                {
                    await AddRoleAsync(roleManager, role);
                }

                // Add admin user
                logger.LogInformation("Seeding admin user.");
                var adminEmail = "sany3y.admin@gmail.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var address = new Address { City = "Cairo", Street = "." };
                    await _addressRepository.Add(address);

                    var adminUser = new User
                    {
                        UserName = "Admin",
                        Email = adminEmail,
                        EmailConfirmed = true,
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Assigning Admin role to the admin user.");
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");

            }

        }

        private static async System.Threading.Tasks.Task AddRoleAsync(RoleManager<Role> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new Role() { Name = roleName });
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
