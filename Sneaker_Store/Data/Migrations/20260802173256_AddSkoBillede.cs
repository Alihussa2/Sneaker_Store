using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sneaker_Store.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSkoBillede : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Billede",
                table: "Sko",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 1,
                column: "Billede",
                value: "https://picsum.photos/seed/nike-air-max/400/300");

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 2,
                column: "Billede",
                value: "https://picsum.photos/seed/asics-gel-1130/400/300");

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 3,
                column: "Billede",
                value: "https://picsum.photos/seed/adidas-campus/400/300");

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 4,
                column: "Billede",
                value: "https://picsum.photos/seed/asics-gel-kayano/400/300");

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 5,
                column: "Billede",
                value: "https://picsum.photos/seed/new-balance-530/400/300");

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 6,
                column: "Billede",
                value: "https://picsum.photos/seed/puma-suede-classic/400/300");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Billede",
                table: "Sko");
        }
    }
}
