using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTransactionOrderCodeFromStringToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OrderCode",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrderCode",
                table: "Transactions",
                column: "OrderCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_OrderHeaders_OrderCode",
                table: "Transactions",
                column: "OrderCode",
                principalTable: "OrderHeaders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_OrderHeaders_OrderCode",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_OrderCode",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
