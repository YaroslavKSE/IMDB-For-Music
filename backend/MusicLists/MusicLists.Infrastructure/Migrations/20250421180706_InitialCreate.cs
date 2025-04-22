using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicLists.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lists",
                columns: table => new
                {
                    ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ListType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ListName = table.Column<string>(type: "text", nullable: false),
                    ListDescription = table.Column<string>(type: "text", nullable: false),
                    IsRanked = table.Column<bool>(type: "boolean", nullable: false),
                    IsScoreDirty = table.Column<bool>(type: "boolean", nullable: false),
                    HotScore = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lists", x => x.ListId);
                });

            migrationBuilder.CreateTable(
                name: "ListComments",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CommentedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommentText = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_ListComments_Lists_ListId",
                        column: x => x.ListId,
                        principalTable: "Lists",
                        principalColumn: "ListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListItems",
                columns: table => new
                {
                    ListItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListItems", x => x.ListItemId);
                    table.ForeignKey(
                        name: "FK_ListItems_Lists_ListId",
                        column: x => x.ListId,
                        principalTable: "Lists",
                        principalColumn: "ListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListLikes",
                columns: table => new
                {
                    LikeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListLikes", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK_ListLikes_Lists_ListId",
                        column: x => x.ListId,
                        principalTable: "Lists",
                        principalColumn: "ListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListComments_ListId",
                table: "ListComments",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_ListComments_ListId_CommentedAt",
                table: "ListComments",
                columns: new[] { "ListId", "CommentedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ListComments_UserId",
                table: "ListComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ListItems_ItemId",
                table: "ListItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ListItems_ListId",
                table: "ListItems",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_ListLikes_ListId",
                table: "ListLikes",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_ListLikes_ListId_UserId",
                table: "ListLikes",
                columns: new[] { "ListId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListLikes_UserId",
                table: "ListLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_HotScore",
                table: "Lists",
                column: "HotScore");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_IsScoreDirty",
                table: "Lists",
                column: "IsScoreDirty");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_UserId",
                table: "Lists",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListComments");

            migrationBuilder.DropTable(
                name: "ListItems");

            migrationBuilder.DropTable(
                name: "ListLikes");

            migrationBuilder.DropTable(
                name: "Lists");
        }
    }
}
