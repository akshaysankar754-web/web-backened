using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBacklogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Backlogs",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Backlogs",
                table: "Users");
        }
    }
}
