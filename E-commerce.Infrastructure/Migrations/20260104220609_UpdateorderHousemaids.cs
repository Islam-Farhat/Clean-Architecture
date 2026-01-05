using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateorderHousemaids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Housemaids_HousemaidId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_HousemaidId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HousemaidId",
                table: "Orders");

            migrationBuilder.CreateTable(
                name: "OrderHousemaid",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    HousemaidId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHousemaid", x => new { x.OrderId, x.HousemaidId });
                    table.ForeignKey(
                        name: "FK_OrderHousemaid_Housemaids_HousemaidId",
                        column: x => x.HousemaidId,
                        principalTable: "Housemaids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderHousemaid_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderHousemaid_HousemaidId",
                table: "OrderHousemaid",
                column: "HousemaidId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderHousemaid");

            migrationBuilder.AddColumn<int>(
                name: "HousemaidId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_HousemaidId",
                table: "Orders",
                column: "HousemaidId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Housemaids_HousemaidId",
                table: "Orders",
                column: "HousemaidId",
                principalTable: "Housemaids",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
