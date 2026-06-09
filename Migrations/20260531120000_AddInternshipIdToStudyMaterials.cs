using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInternshipIdToStudyMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add InternshipId column
            migrationBuilder.AddColumn<int>(
                name: "InternshipId",
                table: "StudyMaterials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Add the InternshipId foreign key constraint
            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterials_InternshipId",
                table: "StudyMaterials",
                column: "InternshipId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyMaterials_Internships_InternshipId",
                table: "StudyMaterials",
                column: "InternshipId",
                principalTable: "Internships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key and index
            migrationBuilder.DropForeignKey(
                name: "FK_StudyMaterials_Internships_InternshipId",
                table: "StudyMaterials");

            migrationBuilder.DropIndex(
                name: "IX_StudyMaterials_InternshipId",
                table: "StudyMaterials");

            // Drop InternshipId column
            migrationBuilder.DropColumn(
                name: "InternshipId",
                table: "StudyMaterials");
        }
    }
}
