using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROGPOE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateUsed",
                table: "ServiceRequests",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalCostZar",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UsdAmount",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExchangeRateUsed",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "LocalCostZar",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "UsdAmount",
                table: "ServiceRequests");
        }
    }
}
