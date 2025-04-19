using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MusicInteraction.Infrastructure.PostgreSQL.Migrations
{
    [DbContext(typeof(MusicInteractionDbContext))]
    [Migration("20250419000001_AddItemStats")]
    public partial class AddItemStats : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemStats",
                columns: table => new
                {
                    ItemId = table.Column<string>(nullable: false),
                    IsRaw = table.Column<bool>(nullable: false),
                    TotalUsersInteracted = table.Column<int>(nullable: false),
                    TotalLikes = table.Column<int>(nullable: false),
                    TotalReviews = table.Column<int>(nullable: false),
                    TotalOneRatings = table.Column<int>(nullable: false),
                    TotalTwoRatings = table.Column<int>(nullable: false),
                    TotalThreeRatings = table.Column<int>(nullable: false),
                    TotalFourRatings = table.Column<int>(nullable: false),
                    TotalFiveRatings = table.Column<int>(nullable: false),
                    TotalSixRatings = table.Column<int>(nullable: false),
                    TotalSevenRatings = table.Column<int>(nullable: false),
                    TotalEightRatings = table.Column<int>(nullable: false),
                    TotalNineRatings = table.Column<int>(nullable: false),
                    TotalTenRatings = table.Column<int>(nullable: false),
                    AverageRating = table.Column<float>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemStats", x => x.ItemId);
                });

            // Create index on ItemId for faster lookups
            migrationBuilder.CreateIndex(
                name: "IX_ItemStats_ItemId",
                table: "ItemStats",
                column: "ItemId");

            // Create index on IsRaw for faster filtering when processing raw stats
            migrationBuilder.CreateIndex(
                name: "IX_ItemStats_IsRaw",
                table: "ItemStats",
                column: "IsRaw",
                filter: "\"IsRaw\" = true");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemStats");
        }
    }
}