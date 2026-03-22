using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Final_project___MaxFitness.Migrations
{
    /// <inheritdoc />
    public partial class LinkWorkoutsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "WorkoutSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_UserId",
                table: "WorkoutSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSessions_AspNetUsers_UserId",
                table: "WorkoutSessions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSessions_AspNetUsers_UserId",
                table: "WorkoutSessions");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessions_UserId",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "WorkoutSessions");
        }
    }
}
