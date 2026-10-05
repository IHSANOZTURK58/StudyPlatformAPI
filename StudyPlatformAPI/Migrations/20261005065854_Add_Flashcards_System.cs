using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class Add_Flashcards_System : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FriendCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfilePublic",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinioFileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Flashcards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FrontSide = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackSide = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepetitionCount = table.Column<int>(type: "int", nullable: false),
                    IntervalInDays = table.Column<int>(type: "int", nullable: false),
                    EasinessFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flashcards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flashcards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KanbanTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReminderSet = table.Column<bool>(type: "bit", nullable: false),
                    ReminderTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    DifficultyWeight = table.Column<int>(type: "int", nullable: false),
                    IsDailyBoss = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KanbanTasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PomodoroSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedDurationInMinutes = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOfflineSynced = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PomodoroSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisualNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MinioImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    X_Coordinate = table.Column<double>(type: "float", nullable: false),
                    Y_Coordinate = table.Column<double>(type: "float", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisualNotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PageNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageNotes_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId",
                table: "Documents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_UserId",
                table: "Flashcards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanTasks_UserId",
                table: "KanbanTasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PageNotes_DocumentId",
                table: "PageNotes",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_UserId",
                table: "PomodoroSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNotes_UserId",
                table: "VisualNotes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flashcards");

            migrationBuilder.DropTable(
                name: "KanbanTasks");

            migrationBuilder.DropTable(
                name: "PageNotes");

            migrationBuilder.DropTable(
                name: "PomodoroSessions");

            migrationBuilder.DropTable(
                name: "VisualNotes");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FriendCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsProfilePublic",
                table: "Users");
        }
    }
}
