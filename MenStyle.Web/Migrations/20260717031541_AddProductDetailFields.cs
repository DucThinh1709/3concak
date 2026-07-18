using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenStyle.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDetailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableColors",
                table: "Products",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvailableSizes",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CareInstruction",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fit",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ShoppingCartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SelectedSize = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SelectedColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingCartItems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingCartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvailableColors", "AvailableSizes", "CareInstruction", "Description", "Fit", "Material", "StockQuantity" },
                values: new object[] { "Đen,Trắng,Nâu,Xám", "S,M,L,XL", "Giặt máy ở chế độ nhẹ, không dùng chất tẩy mạnh, ủi ở nhiệt độ thấp.", "Sản phẩm thời trang nam hiện đại, dễ phối đồ, phù hợp đi học, đi làm và đi chơi.", "Regular fit", "Cotton pha Polyester", 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvailableColors", "AvailableSizes", "CareInstruction", "Description", "Fit", "Material", "StockQuantity" },
                values: new object[] { "Đen,Trắng,Nâu,Xám", "S,M,L,XL", "Giặt máy ở chế độ nhẹ, không dùng chất tẩy mạnh, ủi ở nhiệt độ thấp.", "Sản phẩm thời trang nam hiện đại, dễ phối đồ, phù hợp đi học, đi làm và đi chơi.", "Regular fit", "Cotton pha Polyester", 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AvailableColors", "AvailableSizes", "CareInstruction", "Description", "Fit", "Material", "StockQuantity" },
                values: new object[] { "Đen,Trắng,Nâu,Xám", "S,M,L,XL", "Giặt máy ở chế độ nhẹ, không dùng chất tẩy mạnh, ủi ở nhiệt độ thấp.", "Sản phẩm thời trang nam hiện đại, dễ phối đồ, phù hợp đi học, đi làm và đi chơi.", "Regular fit", "Cotton pha Polyester", 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AvailableColors", "AvailableSizes", "CareInstruction", "Description", "Fit", "Material", "StockQuantity" },
                values: new object[] { "Đen,Trắng,Nâu,Xám", "S,M,L,XL", "Giặt máy ở chế độ nhẹ, không dùng chất tẩy mạnh, ủi ở nhiệt độ thấp.", "Sản phẩm thời trang nam hiện đại, dễ phối đồ, phù hợp đi học, đi làm và đi chơi.", "Regular fit", "Cotton pha Polyester", 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AvailableColors", "AvailableSizes", "CareInstruction", "Description", "Fit", "Material", "StockQuantity" },
                values: new object[] { "Đen,Trắng,Nâu,Xám", "S,M,L,XL", "Giặt máy ở chế độ nhẹ, không dùng chất tẩy mạnh, ủi ở nhiệt độ thấp.", "Sản phẩm thời trang nam hiện đại, dễ phối đồ, phù hợp đi học, đi làm và đi chơi.", "Regular fit", "Cotton pha Polyester", 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AvailableColors", "AvailableSizes", "CareInstruction", "Description", "Fit", "Material", "StockQuantity" },
                values: new object[] { "Đen,Trắng,Nâu,Xám", "S,M,L,XL", "Giặt máy ở chế độ nhẹ, không dùng chất tẩy mạnh, ủi ở nhiệt độ thấp.", "Sản phẩm thời trang nam hiện đại, dễ phối đồ, phù hợp đi học, đi làm và đi chơi.", "Regular fit", "Cotton pha Polyester", 20 });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItems_ProductId",
                table: "ShoppingCartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItems_UserId_ProductId",
                table: "ShoppingCartItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingCartItems");

            migrationBuilder.DropColumn(
                name: "AvailableColors",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AvailableSizes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CareInstruction",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Fit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Products");
        }
    }
}
