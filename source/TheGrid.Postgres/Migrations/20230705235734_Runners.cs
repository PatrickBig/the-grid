// <copyright file="20230705235734_Runners.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;
using TheGrid.Shared.Models;

#nullable disable

namespace TheGrid.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Runners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<string>>(
                name: "Tags",
                table: "Queries",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string[]),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "QueryRunners",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Parameters = table.Column<List<QueryRunnerParameter>>(type: "jsonb", nullable: false),
                    SupportsConnectionTest = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsSchemaDiscovery = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryRunners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataSources_QueryRunnerId",
                table: "DataSources",
                column: "QueryRunnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSources_QueryRunners_QueryRunnerId",
                table: "DataSources",
                column: "QueryRunnerId",
                principalTable: "QueryRunners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataSources_QueryRunners_QueryRunnerId",
                table: "DataSources");

            migrationBuilder.DropTable(
                name: "QueryRunners");

            migrationBuilder.DropIndex(
                name: "IX_DataSources_QueryRunnerId",
                table: "DataSources");

            migrationBuilder.AlterColumn<string[]>(
                name: "Tags",
                table: "Queries",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]");
        }
    }
}
