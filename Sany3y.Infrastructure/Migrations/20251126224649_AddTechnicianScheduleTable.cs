using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sany3y.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianScheduleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TechnicianSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TechnicianId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsBooked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicianSchedules_AspNetUsers_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianSchedules_TechnicianId",
                table: "TechnicianSchedules",
                column: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechnicianSchedules");
        }
    }
}
