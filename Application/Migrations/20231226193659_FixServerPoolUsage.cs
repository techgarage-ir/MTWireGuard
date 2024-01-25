using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTWireGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class FixServerPoolUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseIPPool",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseIPPool",
                table: "Servers");
        }
    }
}
