﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrid.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class EditorLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EditorLanguage",
                table: "QueryRunners",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditorLanguage",
                table: "QueryRunners");
        }
    }
}
