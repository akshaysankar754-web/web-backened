using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInternshipDepartmentMaximumBacklogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaximumBacklogs",
                table: "Internships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Internships",
                type: "longtext",
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumBacklogs",
                table: "Internships");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Internships");
        }
    }
}
