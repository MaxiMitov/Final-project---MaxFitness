using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services;

using Microsoft.EntityFrameworkCore;

namespace MaxFitness.Tests.Tests.Services
{
    public class MuscleServiceTests
    {
        private AppDbContext GetDbContext(string dbName = null)
        {
            dbName ??= Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        // Seed exercises the same way AppDbContext.OnModelCreating does, but in the
        // in-memory DB (seed data via HasData only flows through migrations).
        private static void SeedExercises(AppDbContext db)
        {
            db.Exercises.AddRange(
                new Exercise { Id = 1,  Name = "Bench Press",              MuscleGroup = "chest",    Type = "Compound"   },
                new Exercise { Id = 2,  Name = "Incline Dumbbell Press",   MuscleGroup = "chest",    Type = "Compound"   },
                new Exercise { Id = 3,  Name = "Cable Flyes",              MuscleGroup = "chest",    Type = "Isolation"  },
                new Exercise { Id = 4,  Name = "Push-Ups",                 MuscleGroup = "chest",    Type = "Bodyweight" },
                new Exercise { Id = 5,  Name = "Dumbbell Pullover",        MuscleGroup = "chest",    Type = "Isolation"  },
                new Exercise { Id = 16, Name = "Barbell Curl",             MuscleGroup = "biceps",   Type = "Isolation"  },
                new Exercise { Id = 17, Name = "Hammer Curls",             MuscleGroup = "biceps",   Type = "Isolation"  },
                new Exercise { Id = 21, Name = "Tricep Dips",              MuscleGroup = "triceps",  Type = "Compound"   },
                new Exercise { Id = 22, Name = "Skull Crushers",           MuscleGroup = "triceps",  Type = "Isolation"  }
            );
            db.SaveChanges();
        }

        // ── GetMuscleStatsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task GetMuscleStats_NoLogs_ReturnsPRAsNoData()
        {
            using var db = GetDbContext();
            SeedExercises(db);
            var service = new MuscleService(db);

            var result = await service.GetMuscleStatsAsync("chest", "user-no-logs");

            Assert.Equal("No data", result.CurrentPR);
        }

        [Fact]
        public async Task GetMuscleStats_WithLogs_ReturnsCorrectPR()
        {
            using var db = GetDbContext();
            SeedExercises(db);

            var userId = "user-pr";
            var session = new WorkoutSession { UserId = userId, CompletedAt = DateTime.UtcNow.AddDays(-1), IntensityScore = 70 };
            db.WorkoutSessions.Add(session);
            await db.SaveChangesAsync();

            db.WorkoutExerciseLogs.AddRange(
                new WorkoutExerciseLog { WorkoutSessionId = session.Id, ExerciseName = "Bench Press", MuscleGroup = "chest", SetCount = 4, TotalReps = 32, TotalVolume = 2000 },
                new WorkoutExerciseLog { WorkoutSessionId = session.Id, ExerciseName = "Cable Flyes",  MuscleGroup = "chest", SetCount = 3, TotalReps = 24, TotalVolume = 800  }
            );
            await db.SaveChangesAsync();

            var service = new MuscleService(db);
            var result = await service.GetMuscleStatsAsync("chest", userId);

            Assert.Equal("2000 kg", result.CurrentPR);
        }

        [Fact]
        public async Task GetMuscleStats_ProgressPercentage_GrowsWithSessions()
        {
            using var db = GetDbContext();
            SeedExercises(db);

            var userId = "user-progress";
            // 3 distinct sessions => 3 * 10 = 30%
            for (int i = 1; i <= 3; i++)
            {
                var s = new WorkoutSession { UserId = userId, CompletedAt = DateTime.UtcNow.AddDays(-i), IntensityScore = 50 };
                db.WorkoutSessions.Add(s);
                await db.SaveChangesAsync();
                db.WorkoutExerciseLogs.Add(new WorkoutExerciseLog
                {
                    WorkoutSessionId = s.Id,
                    ExerciseName = "Bench Press",
                    MuscleGroup = "chest",
                    SetCount = 3, TotalReps = 24, TotalVolume = 1200
                });
                await db.SaveChangesAsync();
            }

            var service = new MuscleService(db);
            var result = await service.GetMuscleStatsAsync("chest", userId);

            Assert.Equal(30, result.ProgressPercentage);
        }

        [Fact]
        public async Task GetMuscleStats_TrainedRecentlyUnder24h_RecoveryIsRecovering()
        {
            using var db = GetDbContext();
            SeedExercises(db);

            var userId = "user-recovery";
            var session = new WorkoutSession { UserId = userId, CompletedAt = DateTime.UtcNow.AddHours(-10), IntensityScore = 60 };
            db.WorkoutSessions.Add(session);
            await db.SaveChangesAsync();

            db.WorkoutExerciseLogs.Add(new WorkoutExerciseLog
            {
                WorkoutSessionId = session.Id,
                ExerciseName = "Bench Press",
                MuscleGroup = "chest",
                SetCount = 3, TotalReps = 24, TotalVolume = 1500
            });
            await db.SaveChangesAsync();

            var service = new MuscleService(db);
            var result = await service.GetMuscleStatsAsync("chest", userId);

            Assert.Equal("Recovering", result.RecoveryStatus);
        }

        [Fact]
        public async Task GetMuscleStats_NeverTrained_RecoveryIsReady()
        {
            using var db = GetDbContext();
            SeedExercises(db);
            var service = new MuscleService(db);

            var result = await service.GetMuscleStatsAsync("chest", "fresh-user");

            Assert.Equal("Ready", result.RecoveryStatus);
            Assert.Equal("Anytime", result.NextSession);
        }

        // ── GetExercisesAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task GetExercises_Chest_ReturnsSeededChestExercises()
        {
            using var db = GetDbContext();
            SeedExercises(db);
            var service = new MuscleService(db);

            var exercises = await service.GetExercisesAsync("chest");

            Assert.Equal(5, exercises.Count);
            Assert.Contains(exercises, e => e.Name == "Bench Press");
        }

        [Fact]
        public async Task GetExercises_Arms_ReturnsBicepsAndTricepsCombined()
        {
            using var db = GetDbContext();
            SeedExercises(db);
            var service = new MuscleService(db);

            var exercises = await service.GetExercisesAsync("arms");

            // 2 biceps + 2 triceps seeded above
            Assert.Equal(4, exercises.Count);
            Assert.Contains(exercises, e => e.Name == "Barbell Curl");
            Assert.Contains(exercises, e => e.Name == "Tricep Dips");
        }

        [Fact]
        public async Task GetExercises_NonexistentMuscle_ReturnsEmptyList()
        {
            using var db = GetDbContext();
            SeedExercises(db);
            var service = new MuscleService(db);

            var exercises = await service.GetExercisesAsync("nosuchemuscle");

            Assert.Empty(exercises);
        }
    }
}
