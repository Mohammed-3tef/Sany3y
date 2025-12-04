using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sany3y.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodIdToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PaymentMethodId",
                table: "Tasks",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_PaymentMethods_PaymentMethodId",
                table: "Tasks",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_PaymentMethods_PaymentMethodId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_PaymentMethodId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "Tasks");
        }
    }
}
