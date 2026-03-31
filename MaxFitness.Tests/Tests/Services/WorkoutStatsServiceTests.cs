using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services;

using Microsoft.EntityFrameworkCore;

namespace MaxFitness.Tests.Tests.Services
{
    public class WorkoutStatsServiceTests
    {
        private AppDbContext GetDbContext(string dbName = null)
        {
            dbName ??= Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        // ── GetDashboardStatsAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetDashboardStats_NoWorkouts_ReturnsAllZeros()
        {
            using var db = GetDbContext();
            var service = new WorkoutStatsService(db);

            var stats = await service.GetDashboardStatsAsync("user-1");

            Assert.Equal(0, stats.WeeklyWorkouts);
            Assert.Equal(0, stats.CaloriesThisWeek);
            Assert.Equal(0, stats.AvgIntensity);
            Assert.Equal(0, stats.Streak);
            Assert.Equal(0, stats.WeeklyProgressPercent);
        }

        [Fact]
        public async Task GetDashboardStats_WorkoutsThisWeek_ReturnsCorrectWeeklyCount()
        {
            using var db = GetDbContext();
            var userId = "user-weekly";

            // Calculate start of this week (Monday)
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            if (now.DayOfWeek == DayOfWeek.Sunday) startOfWeek = startOfWeek.AddDays(-7);

            db.WorkoutSessions.AddRange(
                new WorkoutSession { UserId = userId, CompletedAt = startOfWeek.AddHours(10), CaloriesBurned = 200, IntensityScore = 50 },
                new WorkoutSession { UserId = userId, CompletedAt = startOfWeek.AddDays(1), CaloriesBurned = 300, IntensityScore = 60 },
                // old workout — outside current week
                new WorkoutSession { UserId = userId, CompletedAt = startOfWeek.AddDays(-10), CaloriesBurned = 999, IntensityScore = 80 }
            );
            await db.SaveChangesAsync();

            var service = new WorkoutStatsService(db);
            var stats = await service.GetDashboardStatsAsync(userId);

            Assert.Equal(2, stats.WeeklyWorkouts);
        }

        [Fact]
        public async Task GetDashboardStats_CaloriesCalculatedCorrectly()
        {
            using var db = GetDbContext();
            var userId = "user-cal";

            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            if (now.DayOfWeek == DayOfWeek.Sunday) startOfWeek = startOfWeek.AddDays(-7);

            db.WorkoutSessions.AddRange(
                new WorkoutSession { UserId = userId, CompletedAt = startOfWeek.AddHours(1), CaloriesBurned = 400, IntensityScore = 50 },
                new WorkoutSession { UserId = userId, CompletedAt = startOfWeek.AddHours(2), CaloriesBurned = 600, IntensityScore = 50 }
            );
            await db.SaveChangesAsync();

            var service = new WorkoutStatsService(db);
            var stats = await service.GetDashboardStatsAsync(userId);

            Assert.Equal(1000, stats.CaloriesThisWeek);
        }

        // ── GetMuscleStatusesAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetMuscleStatuses_Returns17BodyMapRegions()
        {
            using var db = GetDbContext();
            var service = new WorkoutStatsService(db);

            var statuses = await service.GetMuscleStatusesAsync("any-user");

            Assert.Equal(17, statuses.Count);
        }

        [Fact]
        public async Task GetMuscleStatuses_TrainedChestYesterday_MarksChestAsStrong()
        {
            using var db = GetDbContext();
            var userId = "user-muscle";

            var session = new WorkoutSession
            {
                UserId = userId,
                CompletedAt = DateTime.UtcNow.AddHours(-20),
                IntensityScore = 60
            };
            db.WorkoutSessions.Add(session);
            await db.SaveChangesAsync();

            db.WorkoutExerciseLogs.Add(new WorkoutExerciseLog
            {
                WorkoutSessionId = session.Id,
                ExerciseName = "Bench Press",
                MuscleGroup = "chest",
                SetCount = 3,
                TotalReps = 24,
                TotalVolume = 1500
            });
            await db.SaveChangesAsync();

            var service = new WorkoutStatsService(db);
            var statuses = await service.GetMuscleStatusesAsync(userId);

            var chestLeft = statuses.FirstOrDefault(s => s.Name == "Chest (Left)");
            Assert.NotNull(chestLeft);
            Assert.Equal("strong", chestLeft.Status);
        }

        [Fact]
        public async Task GetMuscleStatuses_NeverTrained_MarksAsNeedsWork()
        {
            using var db = GetDbContext();
            var service = new WorkoutStatsService(db);

            var statuses = await service.GetMuscleStatusesAsync("brand-new-user");

            Assert.All(statuses, s => Assert.Equal("needs-work", s.Status));
        }

        // ── GetRecentWorkoutsAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetRecentWorkouts_NewUser_ReturnsEmptyList()
        {
            using var db = GetDbContext();
            var service = new WorkoutStatsService(db);

            var workouts = await service.GetRecentWorkoutsAsync("new-user", 5);

            Assert.Empty(workouts);
        }

        [Fact]
        public async Task GetRecentWorkouts_ReturnsRequestedCountAndOrderedByMostRecent()
        {
            using var db = GetDbContext();
            var userId = "user-recent";

            for (int i = 1; i <= 7; i++)
            {
                db.WorkoutSessions.Add(new WorkoutSession
                {
                    UserId = userId,
                    CompletedAt = DateTime.UtcNow.AddDays(-i),
                    IntensityScore = 50
                });
            }
            await db.SaveChangesAsync();

            var service = new WorkoutStatsService(db);
            var workouts = await service.GetRecentWorkoutsAsync(userId, 5);

            Assert.Equal(5, workouts.Count);
            // Most recent first
            for (int i = 1; i < workouts.Count; i++)
                Assert.True(workouts[i - 1].CompletedAt >= workouts[i].CompletedAt);
        }

        [Fact]
        public async Task GetRecentWorkouts_DurationMinutes_CalculatedCorrectly()
        {
            using var db = GetDbContext();
            var userId = "user-dur";

            db.WorkoutSessions.Add(new WorkoutSession
            {
                UserId = userId,
                CompletedAt = DateTime.UtcNow.AddHours(-1),
                DurationSeconds = 3660, // 61 minutes
                IntensityScore = 40
            });
            await db.SaveChangesAsync();

            var service = new WorkoutStatsService(db);
            var workouts = await service.GetRecentWorkoutsAsync(userId, 5);

            Assert.Single(workouts);
            Assert.Equal(61, workouts[0].DurationMinutes);
        }

        // ── GetUserProfileStatsAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetUserProfileStats_ReturnsMostTrainedMuscle()
        {
            using var db = GetDbContext();
            var userId = "user-profile";

            var session = new WorkoutSession { UserId = userId, CompletedAt = DateTime.UtcNow.AddDays(-1), IntensityScore = 50 };
            db.WorkoutSessions.Add(session);
            await db.SaveChangesAsync();

            db.WorkoutExerciseLogs.AddRange(
                new WorkoutExerciseLog { WorkoutSessionId = session.Id, ExerciseName = "Bench Press", MuscleGroup = "chest", SetCount = 4, TotalReps = 32, TotalVolume = 2000 },
                new WorkoutExerciseLog { WorkoutSessionId = session.Id, ExerciseName = "Cable Flyes", MuscleGroup = "chest", SetCount = 3, TotalReps = 24, TotalVolume = 800 },
                new WorkoutExerciseLog { WorkoutSessionId = session.Id, ExerciseName = "Barbell Curl", MuscleGroup = "biceps", SetCount = 3, TotalReps = 24, TotalVolume = 600 }
            );
            await db.SaveChangesAsync();

            var service = new WorkoutStatsService(db);
            var stats = await service.GetUserProfileStatsAsync(userId);

            Assert.Equal("Chest", stats.MostTrainedMuscle);
        }

        [Fact]
        public async Task GetUserProfileStats_UnlocksFirstBloodAchievement_AfterOneSession()
        {
            using var db = GetDbContext();
            var userId = "user-achievement";

            db.WorkoutSessions.Add(new WorkoutSession { UserId = userId, CompletedAt = DateTime.UtcNow.AddDays(-1), IntensityScore = 50 });
            await db.SaveChangesAsync();

            var service = new WorkoutStatsService(db);
            var stats = await service.GetUserProfileStatsAsync(userId);

            var firstBlood = stats.Achievements.FirstOrDefault(a => a.Name == "First Blood");
            Assert.NotNull(firstBlood);
            Assert.True(firstBlood.IsUnlocked);
        }

        [Fact]
        public async Task GetUserProfileStats_CenturyClubLocked_WhenFewSessions()
        {
            using var db = GetDbContext();
            var userId = "user-few";

            db.WorkoutSessions.Add(new WorkoutSession { UserId = userId, CompletedAt = DateTime.UtcNow.AddDays(-1), IntensityScore = 30 });
            await db.SaveChangesAsync();

            var service = new WorkoutStatsService(db);
            var stats = await service.GetUserProfileStatsAsync(userId);

            var centuryClub = stats.Achievements.FirstOrDefault(a => a.Name == "Century Club");
            Assert.NotNull(centuryClub);
            Assert.False(centuryClub.IsUnlocked);
        }
    }
}
