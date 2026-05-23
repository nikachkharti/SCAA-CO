using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SCAA_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Inventorry_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.RenameColumn(
                name: "LastLoginDate",
                table: "Orders",
                newName: "OrderDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "Orders",
                newName: "LastLoginDate");

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Inventories",
                columns: new[] { "Id", "AvailableQuantity", "ProductId" },
                values: new object[,]
                {
                    { 1, 48, 1 },
                    { 2, 195, 2 },
                    { 3, 73, 3 },
                    { 4, 27, 4 },
                    { 5, 23, 5 },
                    { 6, 39, 6 },
                    { 7, 96, 7 },
                    { 8, 58, 8 },
                    { 9, 35, 9 },
                    { 10, 42, 10 },
                    { 11, 79, 11 },
                    { 12, 55, 12 },
                    { 13, 65, 13 },
                    { 14, 38, 14 },
                    { 15, 48, 15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId",
                table: "Inventories",
                column: "ProductId");
        }
    }
}
