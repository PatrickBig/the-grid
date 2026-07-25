using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrid.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionLimitsAndTruncation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Truncated",
                table: "QueryExecutions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Truncated",
                table: "QueryExecutions");
        }
    }
}
