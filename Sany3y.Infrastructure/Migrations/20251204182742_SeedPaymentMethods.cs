using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sany3y.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT PaymentMethods ON");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Id = 1) INSERT INTO PaymentMethods (Id, Name, IsActive) VALUES (1, 'Cash', 1)");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Id = 2) INSERT INTO PaymentMethods (Id, Name, IsActive) VALUES (2, 'Online', 1)");
            migrationBuilder.Sql("SET IDENTITY_INSERT PaymentMethods OFF");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
