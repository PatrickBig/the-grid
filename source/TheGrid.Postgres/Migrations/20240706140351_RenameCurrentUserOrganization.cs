// <copyright file="20240706140351_RenameCurrentUserOrganization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrid.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameCurrentUserOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organizations_DefaultOrganizationId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "DefaultOrganizationId",
                table: "AspNetUsers",
                newName: "CurrentOrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_DefaultOrganizationId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_CurrentOrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organizations_CurrentOrganizationId",
                table: "AspNetUsers",
                column: "CurrentOrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organizations_CurrentOrganizationId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CurrentOrganizationId",
                table: "AspNetUsers",
                newName: "DefaultOrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_CurrentOrganizationId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_DefaultOrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organizations_DefaultOrganizationId",
                table: "AspNetUsers",
                column: "DefaultOrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }
    }
}
