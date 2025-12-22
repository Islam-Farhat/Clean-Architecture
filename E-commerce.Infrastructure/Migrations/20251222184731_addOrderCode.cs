using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addOrderCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "0000000000");


            migrationBuilder.Sql(@"
                            WITH OrderedOrders AS (
                                SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS RowNum
                                FROM Orders
                                WHERE OrderCode = '0000000000'
                            )
                            UPDATE Orders
                            SET OrderCode = RIGHT('0000000000' + CAST(RowNum AS NVARCHAR(10)), 10)
                            FROM Orders o
                            INNER JOIN OrderedOrders oo ON o.Id = oo.Id
                            WHERE o.OrderCode = '0000000000'");


            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Orders");
        }
    }
}
