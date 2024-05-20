using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLongToDatetimeInTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionTime",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SuccessTime",
                table: "Transactions");

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionTime",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuccessTime",
                table: "Transactions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionTime",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SuccessTime",
                table: "Transactions");

            migrationBuilder.AddColumn<long>(
                name: "TransactionTime",
                table: "Transactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SuccessTime",
                table: "Transactions",
                type: "bigint",
                nullable: true);
        }
    }
}
