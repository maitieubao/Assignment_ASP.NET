using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment_ASP.NET.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 8,
                column: "ImageUrl",
                value: "/images/iphone14_plus.png");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductID",
                keyValue: 8,
                column: "ImageUrl",
                value: "/images/iphone_14plus.png");
        }
    }
}
