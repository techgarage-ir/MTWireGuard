using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTWireGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class DNS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DNSAddress",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InheritDNS",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InheritIP",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DNSAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InheritDNS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InheritIP",
                table: "Users");
        }
    }
}
