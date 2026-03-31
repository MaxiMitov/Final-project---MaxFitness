using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Final_project___MaxFitness.Migrations
{
    /// <inheritdoc />
    public partial class AddChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    DurationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Rules = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: false),
                    ParticipantCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChallengeId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Challenges",
                columns: new[] { "Id", "Color", "Description", "Difficulty", "DurationDays", "EndDate", "Icon", "Name", "ParticipantCount", "Rules", "StartDate" },
                values: new object[,]
                {
                    { 1, "#e63946", "Complete a squat variation every day for 30 days. Start with bodyweight and progressively add weight. Build insane leg strength and discipline.", "Intermediate", 30, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "fa-person-running", "30-Day Squat Challenge", 0, "Do at least one set of squats every day;Minimum 10 reps per set;Rest days allowed but must make up the next day;Log every session in MaxFitness", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "#ff6b35", "Hit 5,000 steps minimum every single day. A simple but powerful habit that keeps you moving even on rest days. Perfect for active recovery.", "Beginner", 30, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "fa-shoe-prints", "5K Steps Daily", 0, "Walk at least 5,000 steps every day;Track with any step counter or fitness watch;Post your daily count in the community;Miss a day and you're out", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "#ffd166", "Train every single day for 7 days straight. Mix up muscle groups, intensity, and workout types. Test your mental and physical limits.", "Advanced", 7, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "fa-fire", "No Rest Day Week", 0, "Work out every day for 7 consecutive days;Each session must be at least 30 minutes;Vary muscle groups to prevent overtraining;Log all sessions in MaxFitness", new DateTime(2026, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "#22c55e", "Work your way up to 100 push-ups in a single session. Start wherever you are and build up over 4 weeks. Upper body gains guaranteed.", "Intermediate", 28, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "fa-hand-fist", "Push-Up Master", 0, "Do push-ups at least 5 days per week;Track your max set each session;Goal is 100 push-ups in one session by day 28;Any push-up variation counts", new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, "#8b5cf6", "Maintain a 14-day consecutive workout streak. No excuses, no skipping. Build the habit that separates the dedicated from the rest.", "Advanced", 14, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "fa-link", "Iron Streak", 0, "Work out every day for 14 days;Minimum 20 minutes per session;Any workout type counts;Log in MaxFitness to track your streak", new DateTime(2026, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_ChallengeId",
                table: "ChallengeParticipants",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_UserId",
                table: "ChallengeParticipants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengeParticipants");

            migrationBuilder.DropTable(
                name: "Challenges");
        }
    }
}
