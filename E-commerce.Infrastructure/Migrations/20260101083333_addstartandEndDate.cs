using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addstartandEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndShiftDate",
                table: "WorkingDays",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartShiftDate",
                table: "WorkingDays",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndShiftDate",
                table: "WorkingDays");

            migrationBuilder.DropColumn(
                name: "StartShiftDate",
                table: "WorkingDays");
        }
    }
}
