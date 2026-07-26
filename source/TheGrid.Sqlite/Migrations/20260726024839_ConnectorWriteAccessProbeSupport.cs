using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrid.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class ConnectorWriteAccessProbeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SupportsWriteAccessProbe",
                table: "Connectors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupportsWriteAccessProbe",
                table: "Connectors");
        }
    }
}
