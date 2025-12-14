using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sany3y.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedShopDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShop",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShop",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ShopName",
                table: "AspNetUsers");
        }
    }
}
