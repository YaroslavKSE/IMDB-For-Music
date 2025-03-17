using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MusicInteraction.Infrastructure.PostgreSQL.Migrations
{
    [DbContext(typeof(MusicInteractionDbContext))]
    [Migration("20250317000001_InitialCreate")]
    public partial class InitialCreate : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Grades table
            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    MinGrade = table.Column<float>(nullable: false),
                    MaxGrade = table.Column<float>(nullable: false),
                    Grade = table.Column<float>(nullable: true),
                    StepAmount = table.Column<float>(nullable: false),
                    NormalizedGrade = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.EntityId);
                });

            // GradingBlocks table
            migrationBuilder.CreateTable(
                name: "GradingBlocks",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    MinGrade = table.Column<float>(nullable: false),
                    MaxGrade = table.Column<float>(nullable: false),
                    Grade = table.Column<float>(nullable: true),
                    NormalizedGrade = table.Column<float>(nullable: true),
                    ComponentsJson = table.Column<string>(type: "jsonb", nullable: true),
                    ActionsJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingBlocks", x => x.EntityId);
                });

            // GradingMethodInstances table
            migrationBuilder.CreateTable(
                name: "GradingMethodInstances",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(nullable: false),
                    MethodId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    MinGrade = table.Column<float>(nullable: false),
                    MaxGrade = table.Column<float>(nullable: false),
                    Grade = table.Column<float>(nullable: true),
                    NormalizedGrade = table.Column<float>(nullable: true),
                    ComponentsJson = table.Column<string>(type: "jsonb", nullable: true),
                    ActionsJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingMethodInstances", x => x.EntityId);
                });

            // Interactions table
            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    AggregateId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    ItemId = table.Column<string>(nullable: false),
                    ItemType = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    IsLiked = table.Column<bool>(nullable: false),
                    RatingId = table.Column<Guid>(nullable: true),
                    ReviewId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => x.AggregateId);
                });

            // Reviews table
            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<Guid>(nullable: false),
                    ReviewText = table.Column<string>(nullable: false),
                    AggregateId = table.Column<Guid>(nullable: false),
                    ItemId = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ItemType = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_Reviews_Interactions_AggregateId",
                        column: x => x.AggregateId,
                        principalTable: "Interactions",
                        principalColumn: "AggregateId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Ratings table
            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingId = table.Column<Guid>(nullable: false),
                    Grade = table.Column<float>(nullable: true),
                    MinGrade = table.Column<float>(nullable: false),
                    MaxGrade = table.Column<float>(nullable: false),
                    NormalizedGrade = table.Column<float>(nullable: true),
                    IsComplexGrading = table.Column<bool>(nullable: false),
                    AggregateId = table.Column<Guid>(nullable: false),
                    ItemId = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ItemType = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    GradableId = table.Column<Guid>(nullable: true),
                    GradableType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK_Ratings_Interactions_AggregateId",
                        column: x => x.AggregateId,
                        principalTable: "Interactions",
                        principalColumn: "AggregateId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_AggregateId",
                table: "Ratings",
                column: "AggregateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AggregateId",
                table: "Reviews",
                column: "AggregateId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Interactions");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "GradingBlocks");

            migrationBuilder.DropTable(
                name: "GradingMethodInstances");
        }
    }
}