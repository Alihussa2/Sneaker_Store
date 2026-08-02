using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sneaker_Store.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLagerAntalAndSimplifyModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KvitteringId",
                table: "Kvitteringer");

            migrationBuilder.AddColumn<int>(
                name: "LagerAntal",
                table: "Sko",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 1,
                column: "LagerAntal",
                value: 12);

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 2,
                column: "LagerAntal",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 3,
                column: "LagerAntal",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 4,
                column: "LagerAntal",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 5,
                column: "LagerAntal",
                value: 20);

            migrationBuilder.UpdateData(
                table: "Sko",
                keyColumn: "SkoId",
                keyValue: 6,
                column: "LagerAntal",
                value: 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LagerAntal",
                table: "Sko");

            migrationBuilder.AddColumn<int>(
                name: "KvitteringId",
                table: "Kvitteringer",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
