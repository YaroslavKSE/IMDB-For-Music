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
            // 1. First create Interactions table (no foreign keys)
            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    AggregateId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    ItemId = table.Column<string>(nullable: false),
                    ItemType = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    IsLiked = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => x.AggregateId);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingId = table.Column<Guid>(nullable: false),
                    IsComplexGrading = table.Column<bool>(nullable: false),
                    AggregateId = table.Column<Guid>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<Guid>(nullable: false),
                    ReviewText = table.Column<string>(nullable: false),
                    AggregateId = table.Column<Guid>(nullable: false)
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

            // 4. Create GradingBlocks table that doesn't depend on anything else
            migrationBuilder.CreateTable(
                name: "GradingBlocks",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    MinGrade = table.Column<float>(nullable: false),
                    MaxGrade = table.Column<float>(nullable: false),
                    Grade = table.Column<float>(nullable: true),
                    NormalizedGrade = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingBlocks", x => x.EntityId);
                });

            // 5. Create GradingMethodInstances table (dependent on Ratings)
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
                    RatingId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingMethodInstances", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_GradingMethodInstances_Ratings_RatingId",
                        column: x => x.RatingId,
                        principalTable: "Ratings",
                        principalColumn: "RatingId",
                        onDelete: ReferentialAction.Restrict);
                });

            // 6. Create Grades table (dependent on both Ratings and potentially part of a block or method)
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
                    NormalizedGrade = table.Column<float>(nullable: true),
                    RatingId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Grades_Ratings_RatingId",
                        column: x => x.RatingId,
                        principalTable: "Ratings",
                        principalColumn: "RatingId",
                        onDelete: ReferentialAction.Restrict);
                });

            // 7. Create GradingMethodComponents table
            migrationBuilder.CreateTable(
                name: "GradingMethodComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GradingMethodId = table.Column<Guid>(nullable: false),
                    ComponentType = table.Column<string>(nullable: false),
                    ComponentNumber = table.Column<int>(nullable: false),
                    BlockComponentId = table.Column<Guid>(nullable: true),
                    GradeComponentId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingMethodComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingMethodComponents_GradingMethodInstances_GradingMethodId",
                        column: x => x.GradingMethodId,
                        principalTable: "GradingMethodInstances",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GradingMethodComponents_GradingBlocks_BlockComponentId",
                        column: x => x.BlockComponentId,
                        principalTable: "GradingBlocks",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradingMethodComponents_Grades_GradeComponentId",
                        column: x => x.GradeComponentId,
                        principalTable: "Grades",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Restrict);
                });

            // 8. Create GradingBlockComponents table
            migrationBuilder.CreateTable(
                name: "GradingBlockComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GradingBlockId = table.Column<Guid>(nullable: false),
                    ComponentType = table.Column<string>(nullable: false),
                    ComponentNumber = table.Column<int>(nullable: false),
                    BlockComponentId = table.Column<Guid>(nullable: true),
                    GradeComponentId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingBlockComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingBlockComponents_GradingBlocks_GradingBlockId",
                        column: x => x.GradingBlockId,
                        principalTable: "GradingBlocks",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GradingBlockComponents_GradingBlocks_BlockComponentId",
                        column: x => x.BlockComponentId,
                        principalTable: "GradingBlocks",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradingBlockComponents_Grades_GradeComponentId",
                        column: x => x.GradeComponentId,
                        principalTable: "Grades",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Restrict);
                });

            // 9. Create GradingMethodActions table
            migrationBuilder.CreateTable(
                name: "GradingMethodActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GradingMethodId = table.Column<Guid>(nullable: false),
                    ActionNumber = table.Column<int>(nullable: false),
                    ActionType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingMethodActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingMethodActions_GradingMethodInstances_GradingMethodId",
                        column: x => x.GradingMethodId,
                        principalTable: "GradingMethodInstances",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            // 10. Create GradingBlockActions table
            migrationBuilder.CreateTable(
                name: "GradingBlockActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GradingBlockId = table.Column<Guid>(nullable: false),
                    ActionNumber = table.Column<int>(nullable: false),
                    ActionType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingBlockActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingBlockActions_GradingBlocks_GradingBlockId",
                        column: x => x.GradingBlockId,
                        principalTable: "GradingBlocks",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            // 11. Create all necessary indexes
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

            migrationBuilder.CreateIndex(
                name: "IX_Grades_RatingId",
                table: "Grades",
                column: "RatingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingMethodInstances_RatingId",
                table: "GradingMethodInstances",
                column: "RatingId",
                unique: true);

            // Indexes for the new relational tables
            migrationBuilder.CreateIndex(
                name: "IX_GradingMethodComponents_GradingMethodId_ComponentNumber",
                table: "GradingMethodComponents",
                columns: new[] { "GradingMethodId", "ComponentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingMethodComponents_BlockComponentId",
                table: "GradingMethodComponents",
                column: "BlockComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingMethodComponents_GradeComponentId",
                table: "GradingMethodComponents",
                column: "GradeComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingBlockComponents_GradingBlockId_ComponentNumber",
                table: "GradingBlockComponents",
                columns: new[] { "GradingBlockId", "ComponentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingBlockComponents_BlockComponentId",
                table: "GradingBlockComponents",
                column: "BlockComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingBlockComponents_GradeComponentId",
                table: "GradingBlockComponents",
                column: "GradeComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingMethodActions_GradingMethodId_ActionNumber",
                table: "GradingMethodActions",
                columns: new[] { "GradingMethodId", "ActionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingBlockActions_GradingBlockId_ActionNumber",
                table: "GradingBlockActions",
                columns: new[] { "GradingBlockId", "ActionNumber" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order of dependencies
            migrationBuilder.DropTable(
                name: "GradingBlockActions");

            migrationBuilder.DropTable(
                name: "GradingMethodActions");

            migrationBuilder.DropTable(
                name: "GradingBlockComponents");

            migrationBuilder.DropTable(
                name: "GradingMethodComponents");

            migrationBuilder.DropTable(
                name: "GradingMethodInstances");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "GradingBlocks");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Interactions");
        }
    }
}