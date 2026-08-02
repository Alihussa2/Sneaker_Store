using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sneaker_Store.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSko : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Sko",
                columns: new[] { "SkoId", "Maerke", "Model", "Pris", "Str" },
                values: new object[,]
                {
                    { 1, "Nike", "Air Max", 999.0, 44 },
                    { 2, "Asics", "Gel-1130", 850.0, 38 },
                    { 3, "Adidas", "Campus", 700.0, 42 },
                    { 4, "Asics", "Gel-Kayano", 999.0, 44 },
                    { 5, "New Balance", "530", 799.0, 40 },
                    { 6, "Puma", "Suede Classic", 599.0, 43 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 6);
        }
    }
}
