using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SCAA_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    LastLoginDate = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastLoginDate = table.Column<DateTime>(type: "date", nullable: false),
                    OrderAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Electronics" },
                    { 2, "Books" },
                    { 3, "Clothing" },
                    { 4, "Home & Garden" },
                    { 5, "Sports" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "City", "CustomerName", "Email", "LastLoginDate", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "New York", "John Smith", "john.smith@email.com", new DateTime(2026, 5, 18, 0, 0, 0, 0, DateTimeKind.Local), "+1-555-0101" },
                    { 2, "Los Angeles", "Sarah Johnson", "sarah.johnson@email.com", new DateTime(2026, 5, 21, 0, 0, 0, 0, DateTimeKind.Local), "+1-555-0102" },
                    { 3, "Chicago", "Michael Brown", "michael.brown@email.com", new DateTime(2026, 5, 23, 0, 0, 0, 0, DateTimeKind.Local), "+1-555-0103" },
                    { 4, "Houston", "Emily Davis", "emily.davis@email.com", new DateTime(2026, 5, 13, 0, 0, 0, 0, DateTimeKind.Local), "+1-555-0104" },
                    { 5, "Phoenix", "David Wilson", "david.wilson@email.com", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Local), "+1-555-0105" }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "SupplierName" },
                values: new object[,]
                {
                    { 1, "TechCorp Electronics" },
                    { 2, "Global Publishing Inc." },
                    { 3, "Fashion World Ltd." },
                    { 4, "Home Essentials Co." },
                    { 5, "Sports Gear Supply" }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CustomerId", "Discount", "LastLoginDate", "OrderAmount", "Status" },
                values: new object[,]
                {
                    { 1, 1, 10m, new DateTime(2026, 5, 19, 0, 0, 0, 0, DateTimeKind.Local), 199.97m, "Delivered" },
                    { 2, 2, 15m, new DateTime(2026, 5, 22, 0, 0, 0, 0, DateTimeKind.Local), 369.96m, "Processing" },
                    { 3, 3, 5m, new DateTime(2026, 5, 23, 0, 0, 0, 0, DateTimeKind.Local), 249.97m, "Pending" },
                    { 4, 1, 0m, new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Local), 129.98m, "Delivered" },
                    { 5, 4, 20m, new DateTime(2026, 5, 21, 0, 0, 0, 0, DateTimeKind.Local), 389.97m, "Shipped" },
                    { 6, 5, 8m, new DateTime(2026, 5, 17, 0, 0, 0, 0, DateTimeKind.Local), 149.98m, "Delivered" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Price", "ProductName", "Quantity", "SupplierId" },
                values: new object[,]
                {
                    { 1, 1, 79.99m, "Wireless Headphones", 50, 1 },
                    { 2, 1, 12.99m, "USB-C Cable", 200, 1 },
                    { 3, 1, 49.99m, "Portable Charger", 75, 1 },
                    { 4, 2, 45.99m, "C# Programming Guide", 30, 2 },
                    { 5, 2, 55.99m, "Entity Framework Core in Action", 25, 2 },
                    { 6, 2, 39.99m, "Clean Code", 40, 2 },
                    { 7, 3, 19.99m, "Cotton T-Shirt", 100, 3 },
                    { 8, 3, 59.99m, "Denim Jeans", 60, 3 },
                    { 9, 3, 129.99m, "Winter Jacket", 35, 3 },
                    { 10, 4, 34.99m, "LED Desk Lamp", 45, 4 },
                    { 11, 4, 24.99m, "Plant Pot Set", 80, 4 },
                    { 12, 4, 29.99m, "Wall Clock", 55, 4 },
                    { 13, 5, 25.99m, "Yoga Mat", 70, 5 },
                    { 14, 5, 89.99m, "Dumbbell Set", 40, 5 },
                    { 15, 5, 99.99m, "Running Shoes", 50, 5 }
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

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "OrderId", "PricePerUnit", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 79.99m, 1, 2 },
                    { 2, 1, 12.99m, 2, 5 },
                    { 3, 2, 45.99m, 4, 3 },
                    { 4, 2, 55.99m, 5, 2 },
                    { 5, 3, 19.99m, 7, 4 },
                    { 6, 3, 59.99m, 8, 2 },
                    { 7, 4, 34.99m, 10, 3 },
                    { 8, 4, 24.99m, 11, 1 },
                    { 9, 5, 25.99m, 13, 5 },
                    { 10, 5, 89.99m, 14, 2 },
                    { 11, 6, 49.99m, 3, 2 },
                    { 12, 6, 39.99m, 6, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId",
                table: "Inventories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
