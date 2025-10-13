using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Sany3y.Infrastructure.Models
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<UserPhone> UserPhones { get; set; }
        public DbSet<ProfilePicture> ProfilePictures { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define composite key for UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
        }
    }
}
