using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedUsernamefield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. First add the Username column without the unique index
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "temp_user");
                
            // 2. Update all existing records to have unique usernames based on their email and ID
            migrationBuilder.Sql(@"
                UPDATE users 
                SET ""Username"" = CONCAT(
                    CASE 
                        WHEN POSITION('@' IN ""Email"") > 1 THEN SUBSTRING(""Email"", 1, POSITION('@' IN ""Email"") - 1)
                        ELSE 'user'
                    END,
                    '_',
                    SUBSTRING(CAST(""Id"" AS VARCHAR), 1, 8)
                );
            ");
            
            // 3. After all records have unique values, create the unique index
            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Username",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "users");
        }
    }
}