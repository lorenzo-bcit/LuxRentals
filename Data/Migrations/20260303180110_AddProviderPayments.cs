using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxRentals.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderPayment",
                columns: table => new
                {
                    pkPaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkTransactionId = table.Column<int>(type: "int", nullable: false),
                    paymentProvider = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    paymentProviderOrderId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    paymentProviderCaptureId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    receivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    rawWebHookJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderPayment", x => x.pkPaymentId);
                    table.ForeignKey(
                        name: "FK_ProviderPayment_Transaction",
                        column: x => x.fkTransactionId,
                        principalTable: "Transaction",
                        principalColumn: "pkTransactionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderPayment_fkTransactionId",
                table: "ProviderPayment",
                column: "fkTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderPayment");
        }
    }
}
