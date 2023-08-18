// <copyright file="20230604213725_Initial.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TheGrid.Models;
using TheGrid.Shared.Models;

#nullable disable

namespace TheGrid.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    QueryRunnerId = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ExecutorParameters = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSources_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Queries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSourceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Command = table.Column<string>(type: "text", nullable: false),
                    Parameters = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    Tags = table.Column<string[]>(type: "text[]", nullable: true),
                    ResultsRefreshed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Columns = table.Column<Dictionary<string, QueryResultColumn>>(type: "jsonb", nullable: true),
                    ResultState = table.Column<int>(type: "integer", nullable: false),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Queries_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueryResultRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueryId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryResultRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryResultRows_Queries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "Queries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataSources_OrganizationId",
                table: "DataSources",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Queries_DataSourceId",
                table: "Queries",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryResultRows_QueryId",
                table: "QueryResultRows",
                column: "QueryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueryResultRows");

            migrationBuilder.DropTable(
                name: "Queries");

            migrationBuilder.DropTable(
                name: "DataSources");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
