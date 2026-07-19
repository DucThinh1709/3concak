using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenStyle.Web.Migrations
{
    /// <inheritdoc />
    public partial class SyncManualDatabaseChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedImageUrl",
                table: "ShoppingCartItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ShippingLatitude",
                table: "CustomerOrders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShippingLongitude",
                table: "CustomerOrders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedImageUrl",
                table: "CustomerOrderItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedImageUrl",
                table: "ShoppingCartItems");

            migrationBuilder.DropColumn(
                name: "ShippingLatitude",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "ShippingLongitude",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "SelectedImageUrl",
                table: "CustomerOrderItems");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AspNetUsers");
        }
    }
}
