using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTWireGuard.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddServersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DNSAddress = table.Column<string>(type: "TEXT", nullable: true),
                    InheritDNS = table.Column<bool>(type: "INTEGER", nullable: false),
                    IPPoolId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
