using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sany3y.Infrastructure.Models
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            //  هنا connection string بتاعك
            optionsBuilder.UseSqlServer(
                "Server=.;Database=Sany3yDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
