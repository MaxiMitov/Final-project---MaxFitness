using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Final_project___MaxFitness.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    IntensityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    CaloriesBurned = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalVolume = table.Column<double>(type: "REAL", nullable: false),
                    TotalSets = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalReps = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutExerciseLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseName = table.Column<string>(type: "TEXT", nullable: false),
                    MuscleGroup = table.Column<string>(type: "TEXT", nullable: false),
                    SetsJson = table.Column<string>(type: "TEXT", nullable: false),
                    SetCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalReps = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalVolume = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutExerciseLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutExerciseLogs_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExerciseLogs_WorkoutSessionId",
                table: "WorkoutExerciseLogs",
                column: "WorkoutSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkoutExerciseLogs");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");
        }
    }
}
