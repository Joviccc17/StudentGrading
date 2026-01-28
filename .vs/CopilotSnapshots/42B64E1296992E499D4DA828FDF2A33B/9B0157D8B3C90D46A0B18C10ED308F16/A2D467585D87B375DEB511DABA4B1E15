using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentGrading0._5.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToExamResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, delete any existing exam results (orphaned data without valid user)
            migrationBuilder.Sql("DELETE FROM ExamResults");

            // Add the UserId column with nullable type first
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 1); // Temporary default to first user

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_UserId",
                table: "ExamResults",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Users_UserId",
                table: "ExamResults",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Users_UserId",
                table: "ExamResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamResults_UserId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExamResults");
        }
    }
}
