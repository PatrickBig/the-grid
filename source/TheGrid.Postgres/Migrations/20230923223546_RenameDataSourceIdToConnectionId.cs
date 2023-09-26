using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrid.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameDataSourceIdToConnectionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queries_Connections_ConnectionId",
                table: "Queries");

            migrationBuilder.DropColumn(
                name: "DataSourceId",
                table: "Queries");

            migrationBuilder.AlterColumn<int>(
                name: "ConnectionId",
                table: "Queries",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Queries_Connections_ConnectionId",
                table: "Queries",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queries_Connections_ConnectionId",
                table: "Queries");

            migrationBuilder.AlterColumn<int>(
                name: "ConnectionId",
                table: "Queries",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "DataSourceId",
                table: "Queries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Queries_Connections_ConnectionId",
                table: "Queries",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "Id");
        }
    }
}
