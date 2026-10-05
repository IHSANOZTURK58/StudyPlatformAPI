using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class Finalize_Dashboard_And_Kanban : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "KanbanTasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "KanbanTasks");
        }
    }
}
