using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTWireGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class UserTrafficLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemReset",
                table: "DataUsages");

            migrationBuilder.AddColumn<int>(
                name: "TrafficLimit",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrafficLimit",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "SystemReset",
                table: "DataUsages",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
