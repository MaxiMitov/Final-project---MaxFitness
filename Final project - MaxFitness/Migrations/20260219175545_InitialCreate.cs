using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Final_project___MaxFitness.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MuscleGroup = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "MuscleGroup", "Name", "Type" },
                values: new object[,]
                {
                    { 1, "chest", "Bench Press", "Compound" },
                    { 2, "chest", "Incline Dumbbell Press", "Compound" },
                    { 3, "chest", "Cable Flyes", "Isolation" },
                    { 4, "chest", "Push-Ups", "Bodyweight" },
                    { 5, "chest", "Dumbbell Pullover", "Isolation" },
                    { 6, "back", "Deadlift", "Compound" },
                    { 7, "back", "Lat Pulldown", "Compound" },
                    { 8, "back", "Barbell Row", "Compound" },
                    { 9, "back", "Seated Cable Row", "Isolation" },
                    { 10, "back", "Face Pulls", "Isolation" },
                    { 11, "shoulders", "Overhead Press", "Compound" },
                    { 12, "shoulders", "Lateral Raises", "Isolation" },
                    { 13, "shoulders", "Front Raises", "Isolation" },
                    { 14, "shoulders", "Reverse Flyes", "Isolation" },
                    { 15, "shoulders", "Arnold Press", "Compound" },
                    { 16, "biceps", "Barbell Curl", "Isolation" },
                    { 17, "biceps", "Hammer Curls", "Isolation" },
                    { 18, "biceps", "Incline Dumbbell Curls", "Isolation" },
                    { 19, "biceps", "Preacher Curls", "Isolation" },
                    { 20, "biceps", "Cable Curls", "Isolation" },
                    { 21, "triceps", "Tricep Dips", "Compound" },
                    { 22, "triceps", "Skull Crushers", "Isolation" },
                    { 23, "triceps", "Tricep Pushdown", "Isolation" },
                    { 24, "triceps", "Overhead Tricep Extension", "Isolation" },
                    { 25, "triceps", "Close-Grip Bench Press", "Compound" },
                    { 26, "legs", "Barbell Squat", "Compound" },
                    { 27, "legs", "Romanian Deadlift", "Compound" },
                    { 28, "legs", "Leg Press", "Compound" },
                    { 29, "legs", "Leg Curl", "Isolation" },
                    { 30, "legs", "Calf Raises", "Isolation" },
                    { 31, "legs", "Bulgarian Split Squat", "Compound" },
                    { 32, "abs", "Crunches", "Bodyweight" },
                    { 33, "abs", "Hanging Leg Raises", "Bodyweight" },
                    { 34, "abs", "Plank", "Bodyweight" },
                    { 35, "abs", "Cable Woodchops", "Isolation" },
                    { 36, "abs", "Ab Wheel Rollout", "Bodyweight" },
                    { 37, "forearms", "Wrist Curls", "Isolation" },
                    { 38, "forearms", "Reverse Wrist Curls", "Isolation" },
                    { 39, "forearms", "Farmer's Walk", "Compound" },
                    { 40, "forearms", "Plate Pinch", "Isolation" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");
        }
    }
}
