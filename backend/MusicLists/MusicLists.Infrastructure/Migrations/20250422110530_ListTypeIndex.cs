using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicLists.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ListTypeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Lists_ListType",
                table: "Lists",
                column: "ListType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lists_ListType",
                table: "Lists");
        }
    }
}
