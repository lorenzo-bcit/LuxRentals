using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxRentals.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedTransactionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderPayment_Transaction",
                table: "ProviderPayment");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_ProviderPayment_fkTransactionId",
                table: "ProviderPayment");

            migrationBuilder.RenameColumn(
                name: "fkTransactionId",
                table: "ProviderPayment",
                newName: "FkTransactionId");

            migrationBuilder.AddColumn<string>(
                name: "transactionId",
                table: "Booking",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transactionId",
                table: "Booking");

            migrationBuilder.RenameColumn(
                name: "FkTransactionId",
                table: "ProviderPayment",
                newName: "fkTransactionId");

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    pkTransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkBookingId = table.Column<int>(type: "int", nullable: false),
                    amountPaid = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    paymentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transact__D335440639A81962", x => x.pkTransactionId);
                    table.ForeignKey(
                        name: "FK_Transaction_Booking",
                        column: x => x.fkBookingId,
                        principalTable: "Booking",
                        principalColumn: "pkBookingId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderPayment_fkTransactionId",
                table: "ProviderPayment",
                column: "fkTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_fkBookingId",
                table: "Transaction",
                column: "fkBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderPayment_Transaction",
                table: "ProviderPayment",
                column: "fkTransactionId",
                principalTable: "Transaction",
                principalColumn: "pkTransactionId");
        }
    }
}
