using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sneaker_Store.Data.Migrations
{
    /// <inheritdoc />
    public partial class PromoteAdminInExtraDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE `Kunder` SET `IsAdmin` = 1 WHERE `Email` = 'test@sneakerstore.dk';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE `Kunder` SET `IsAdmin` = 0 WHERE `Email` = 'test@sneakerstore.dk';");
        }
    }
}
