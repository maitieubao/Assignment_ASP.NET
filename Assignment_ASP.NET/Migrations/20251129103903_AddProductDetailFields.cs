using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment_ASP.NET.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDetailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyFeatures",
                table: "Products",
                type: "ntext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specifications",
                table: "Products",
                type: "ntext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyPeriod",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 1,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 2,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 3,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 4,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 5,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 6,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 7,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 8,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 9,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 10,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 11,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 12,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 13,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 14,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 15,
                columns: new[] { "KeyFeatures", "Manufacturer", "Specifications", "WarrantyPeriod" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeyFeatures",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Specifications",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WarrantyPeriod",
                table: "Products");
        }
    }
}
