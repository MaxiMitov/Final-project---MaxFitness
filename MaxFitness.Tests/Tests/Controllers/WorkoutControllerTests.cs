using Final_project___MaxFitness.Controllers;
using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace MaxFitness.Tests.Tests.Controllers
{
    public class WorkoutControllerTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private AppDbContext GetDbContext(string dbName = null)
        {
            dbName ??= Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        private Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private WorkoutController BuildController(
            AppDbContext db,
            Mock<UserManager<IdentityUser>> userMgr,
            string userId = "test-user")
        {
            userMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                   .Returns(userId);

            var controller = new WorkoutController(db, userMgr.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return controller;
        }

        private static void SeedChestExercises(AppDbContext db)
        {
            db.Exercises.AddRange(
                new Exercise { Id = 1, Name = "Bench Press",            MuscleGroup = "chest", Type = "Compound"   },
                new Exercise { Id = 2, Name = "Incline Dumbbell Press", MuscleGroup = "chest", Type = "Compound"   },
                new Exercise { Id = 3, Name = "Cable Flyes",            MuscleGroup = "chest", Type = "Isolation"  },
                new Exercise { Id = 4, Name = "Push-Ups",               MuscleGroup = "chest", Type = "Bodyweight" }
            );
            db.SaveChanges();
        }

        // ── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public void Index_ReturnsViewResult()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            var controller = BuildController(db, userMgr);

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        // ── GetExercises ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetExercises_ValidMuscleGroup_ReturnsJsonWithExercises()
        {
            using var db = GetDbContext();
            SeedChestExercises(db);

            var userMgr = GetMockUserManager();
            var controller = BuildController(db, userMgr);

            var result = await controller.GetExercises("chest");

            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
            // Cast to enumerable and count
            var list = json.Value as IEnumerable<object> ?? (json.Value as System.Collections.IEnumerable)?.Cast<object>();
            Assert.NotNull(list);
            Assert.Equal(4, list.Count());
        }

        [Fact]
        public async Task GetExercises_InvalidMuscleGroup_ReturnsEmptyJson()
        {
            using var db = GetDbContext();
            SeedChestExercises(db);

            var userMgr = GetMockUserManager();
            var controller = BuildController(db, userMgr);

            var result = await controller.GetExercises("doesnotexist");

            var json = Assert.IsType<JsonResult>(result);
            var list = json.Value as System.Collections.IEnumerable;
            Assert.NotNull(list);
            Assert.Empty(list.Cast<object>());
        }

        // ── SaveWorkout ───────────────────────────────────────────────────────

        [Fact]
        public async Task SaveWorkout_ValidRequest_CreatesSessionAndLogsInDb()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            var controller = BuildController(db, userMgr, "workout-user");

            var request = new SaveWorkoutRequest
            {
                DurationSeconds = 3600,
                IntensityScore = 60,
                TotalSets = 12,
                TotalReps = 96,
                TotalVolume = 4800,
                Exercises = new List<WorkoutExerciseDto>
                {
                    new WorkoutExerciseDto
                    {
                        Name = "Bench Press",
                        Muscle = "chest",
                        Sets = new List<SetDto>
                        {
                            new SetDto { Reps = "8", Weight = "80" },
                            new SetDto { Reps = "8", Weight = "80" },
                            new SetDto { Reps = "6", Weight = "85" }
                        }
                    }
                }
            };

            var result = await controller.SaveWorkout(request);

            Assert.IsType<JsonResult>(result);
            Assert.Equal(1, await db.WorkoutSessions.CountAsync());
            Assert.Equal(1, await db.WorkoutExerciseLogs.CountAsync());

            var session = await db.WorkoutSessions.FirstAsync();
            Assert.Equal("workout-user", session.UserId);
            Assert.Equal(3600, session.DurationSeconds);
            Assert.Equal(60, session.IntensityScore);
        }

        [Fact]
        public async Task SaveWorkout_EmptyExerciseList_ReturnsBadRequest()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            var controller = BuildController(db, userMgr);

            var request = new SaveWorkoutRequest
            {
                DurationSeconds = 600,
                IntensityScore = 30,
                Exercises = new List<WorkoutExerciseDto>()
            };

            var result = await controller.SaveWorkout(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
