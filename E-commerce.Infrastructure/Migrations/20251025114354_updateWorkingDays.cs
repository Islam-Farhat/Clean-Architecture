using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateWorkingDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "WorkingDays",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkingDays_DriverId",
                table: "WorkingDays",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkingDays_Users_DriverId",
                table: "WorkingDays",
                column: "DriverId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkingDays_Users_DriverId",
                table: "WorkingDays");

            migrationBuilder.DropIndex(
                name: "IX_WorkingDays_DriverId",
                table: "WorkingDays");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "WorkingDays");
        }
    }
}
