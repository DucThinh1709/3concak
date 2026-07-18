using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenStyle.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProductColorImageMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ShoppingCartItems_UserId_ProductId'
      AND object_id = OBJECT_ID('dbo.ShoppingCartItems')
)
BEGIN
    DROP INDEX IX_ShoppingCartItems_UserId_ProductId
    ON dbo.ShoppingCartItems;
END
");

            migrationBuilder.AddColumn<string>(
                name: "ColorImageMap",
                table: "Products",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedColor",
                table: "CustomerOrderItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedSize",
                table: "CustomerOrderItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "ColorImageMap",
                value: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "ColorImageMap",
                value: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "ColorImageMap",
                value: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "ColorImageMap",
                value: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "ColorImageMap",
                value: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "ColorImageMap",
                value: "");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ShoppingCartItems_UserId_ProductId_SelectedSize_SelectedColor'
      AND object_id = OBJECT_ID('dbo.ShoppingCartItems')
)
BEGIN
    CREATE UNIQUE INDEX IX_ShoppingCartItems_UserId_ProductId_SelectedSize_SelectedColor
    ON dbo.ShoppingCartItems(UserId, ProductId, SelectedSize, SelectedColor);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShoppingCartItems_UserId_ProductId_SelectedSize_SelectedColor",
                table: "ShoppingCartItems");

            migrationBuilder.DropColumn(
                name: "ColorImageMap",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SelectedColor",
                table: "CustomerOrderItems");

            migrationBuilder.DropColumn(
                name: "SelectedSize",
                table: "CustomerOrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItems_UserId_ProductId",
                table: "ShoppingCartItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }
    }
}
