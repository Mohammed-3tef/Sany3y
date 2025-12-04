using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sany3y.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleIdToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ScheduleId",
                table: "Tasks",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TechnicianSchedules_ScheduleId",
                table: "Tasks",
                column: "ScheduleId",
                principalTable: "TechnicianSchedules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TechnicianSchedules_ScheduleId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ScheduleId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Tasks");
        }
    }
}
