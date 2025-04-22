using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MusicInteraction.Infrastructure.PostgreSQL.Migrations
{
    [DbContext(typeof(MusicInteractionDbContext))]
    [Migration("20250421000001_AddReviewHotScore")]
    public partial class AddReviewHotScore : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add HotScore column with default value 0
            migrationBuilder.AddColumn<float>(
                name: "HotScore",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            // Add IsScoreDirty column with default value false
            migrationBuilder.AddColumn<bool>(
                name: "IsScoreDirty",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Create index on HotScore for faster sorting
            migrationBuilder.CreateIndex(
                name: "IX_Reviews_HotScore",
                table: "Reviews",
                column: "HotScore");

            // Create index on IsScoreDirty to quickly find reviews needing recalculation
            migrationBuilder.CreateIndex(
                name: "IX_Reviews_IsScoreDirty",
                table: "Reviews",
                column: "IsScoreDirty",
                filter: "\"IsScoreDirty\" = true");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the indexes
            migrationBuilder.DropIndex(
                name: "IX_Reviews_HotScore",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_IsScoreDirty",
                table: "Reviews");

            // Drop the columns
            migrationBuilder.DropColumn(
                name: "HotScore",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsScoreDirty",
                table: "Reviews");
        }
    }
}