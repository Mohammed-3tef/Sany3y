using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly List<string> Roles = new() { "Admin", "Tasker", "Client" };

        public static async System.Threading.Tasks.Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var addressRepository = scope.ServiceProvider.GetRequiredService<IRepository<Address>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                //logger.LogInformation("Ensuring the database is created...");
                //await context.Database.EnsureCreatedAsync();

                // Seed roles
                logger.LogInformation("Seeding roles...");
                await SeedRolesAsync(roleManager, logger);

                // Seed admin user
                logger.LogInformation("Seeding admin user...");
                await SeedAdminUserAsync(userManager, addressRepository, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async System.Threading.Tasks.Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger logger)
        {
            foreach (var roleName in Roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new Role { Name = roleName });
                    if (result.Succeeded)
                        logger.LogInformation($"✅ Role '{roleName}' created successfully.");
                    else
                        logger.LogError($"❌ Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static async System.Threading.Tasks.Task SeedAdminUserAsync(UserManager<User> userManager, IRepository<Address> addressRepository, ILogger logger)
        {
            const string adminEmail = "sany3y.admin@gmail.com";
            const string adminPassword = "Admin@123";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                logger.LogInformation("Admin user already exists.");
                return;
            }

            var address = new Address { City = "Cairo", Street = "Cairo" };
            await addressRepository.Add(address);

            var adminUser = new User
            {
                NationalId = 10000000000001,
                FirstName = "Sany3y",
                LastName = "Admin",
                UserName = "Admin",
                Email = adminEmail,
                Gender = 'M',
                BirthDate = new DateTime(2005, 11, 16),
                PhoneNumber = ".",
                AddressId = address.Id,
                PasswordHash = adminPassword,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user created and assigned 'Admin' role.");
            }
            else
            {
                logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
