using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenStyle.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentInfoToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "CustomerOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "CustomerOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "CustomerOrders");
        }
    }
}
