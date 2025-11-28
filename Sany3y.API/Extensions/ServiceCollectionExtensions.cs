using Microsoft.AspNetCore.Identity.UI.Services;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.Services;
using Sany3y.API.Services.CountryServices;

namespace Sany3y.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Provinces, Governorates, and Cities Services
            services.AddScoped<ProvinceServices>();
            services.AddScoped<GovernorateServices>();
            services.AddScoped<CityServices>();

            services.AddScoped<IEmailSender, EmailConfirm>();
            services.AddScoped<UserRepository, UserRepository>();
            services.AddScoped<IRepository<Address>, AddressRepository>();
            services.AddScoped<IRepository<Category>, CategoryRepository>();
            services.AddScoped<IRepository<Message>, MessageRepository>();
            services.AddScoped<IRepository<ProfilePicture>, ProfilePictureRepository>();
            services.AddScoped<IRepository<Notification>, NotificationRepository>();
            services.AddScoped<IRepository<Rating>, RatingRepository>();
            //services.AddScoped<IRepository<PaymentMethod>, PaymentMethodRepository>();
            //services.AddScoped<IRepository<Payment>, PaymentRepository>();
            services.AddScoped<IRepository<Infrastructure.Models.Task>, TaskRepository>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}