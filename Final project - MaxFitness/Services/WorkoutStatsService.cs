using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;

using Microsoft.EntityFrameworkCore;

namespace Final_project___MaxFitness.Services
{
    public class WorkoutStatsService : IWorkoutStatsService
    {
        private readonly AppDbContext _context;

        private static readonly Dictionary<string, string[]> MuscleMapping = new()
        {
            ["chest"] = new[] { "Chest (Left)", "Chest (Right)" },
            ["back"] = new[] { "Traps" },
            ["shoulders"] = new[] { "Left Shoulder", "Right Shoulder", "Neck" },
            ["biceps"] = new[] { "Left Bicep", "Right Bicep" },
            ["legs"] = new[] { "Left Quad", "Right Quad", "Left Calf", "Right Calf" },
            ["abs"] = new[] { "Abs", "Left Oblique", "Right Oblique" },
            ["forearms"] = new[] { "Left Forearm", "Right Forearm" }
        };

        private static readonly string[] AllBodyMapRegions = new[]
        {
            "Neck", "Traps",
            "Left Shoulder", "Right Shoulder",
            "Chest (Left)", "Chest (Right)",
            "Left Bicep", "Right Bicep",
            "Left Forearm", "Right Forearm",
            "Abs", "Left Oblique", "Right Oblique",
            "Left Quad", "Right Quad",
            "Left Calf", "Right Calf"
        };

        public WorkoutStatsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(string userId)
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            if (now.DayOfWeek == DayOfWeek.Sunday)
                startOfWeek = startOfWeek.AddDays(-7);

            var weekWorkouts = await _context.WorkoutSessions
                .Where(w => w.UserId == userId && w.CompletedAt >= startOfWeek)
                .ToListAsync();

            var weeklyCount = weekWorkouts.Count;
            var goal = 6;
            var progressPercent = Math.Min(100, (int)Math.Round((weeklyCount / (double)goal) * 100));
            var caloriesThisWeek = weekWorkouts.Sum(w => w.CaloriesBurned);

            var recentIntensities = await _context.WorkoutSessions
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CompletedAt)
                .Take(10)
                .Select(w => w.IntensityScore)
                .ToListAsync();

            var avgIntensity = recentIntensities.Count > 0
                ? (int)Math.Round(recentIntensities.Average())
                : 0;

            var streak = await CalculateStreakAsync(userId);

            return new DashboardStats
            {
                WeeklyWorkouts = weeklyCount,
                WeeklyGoal = goal,
                WeeklyProgressPercent = progressPercent,
                Streak = streak,
                CaloriesThisWeek = caloriesThisWeek,
                AvgIntensity = avgIntensity
            };
        }

        private async Task<int> CalculateStreakAsync(string userId)
        {
            var workoutDates = await _context.WorkoutSessions
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CompletedAt)
                .Select(w => w.CompletedAt.Date)
                .ToListAsync();

            if (workoutDates.Count == 0) return 0;

            var uniqueDates = workoutDates.Distinct().OrderByDescending(d => d).ToList();
            var today = DateTime.UtcNow.Date;

            if (uniqueDates[0] < today.AddDays(-1)) return 0;

            var streak = 1;
            for (int i = 1; i < uniqueDates.Count; i++)
            {
                if ((uniqueDates[i - 1] - uniqueDates[i]).Days == 1)
                    streak++;
                else
                    break;
            }

            return streak;
        }

        public async Task<List<MuscleStatus>> GetMuscleStatusesAsync(string userId)
        {
            var now = DateTime.UtcNow;

            var logs = await _context.WorkoutExerciseLogs
                .Include(l => l.WorkoutSession)
                .Where(l => l.WorkoutSession.UserId == userId)
                .OrderByDescending(l => l.WorkoutSession.CompletedAt)
                .ToListAsync();

            var lastTrainedByGroup = new Dictionary<string, DateTime>();
            foreach (var log in logs)
            {
                var group = log.MuscleGroup.ToLower();
                if (!lastTrainedByGroup.ContainsKey(group))
                    lastTrainedByGroup[group] = log.WorkoutSession.CompletedAt;
            }

            var statuses = new List<MuscleStatus>();
            foreach (var region in AllBodyMapRegions)
            {
                var matchedGroup = MuscleMapping
                    .Where(kv => kv.Value.Contains(region))
                    .Select(kv => kv.Key)
                    .FirstOrDefault();

                string status = "needs-work";
                string lastTrained = "Never";
                int exerciseCount = 0;

                if (matchedGroup != null && lastTrainedByGroup.TryGetValue(matchedGroup, out var trainedAt))
                {
                    var daysSince = (now - trainedAt).TotalDays;
                    if (daysSince <= 2)
                        status = "strong";
                    else if (daysSince <= 5)
                        status = "moderate";
                    else
                        status = "needs-work";

                    lastTrained = FormatTimeAgo(trainedAt);

                    exerciseCount = logs.Count(l => l.MuscleGroup.ToLower() == matchedGroup);
                }

                statuses.Add(new MuscleStatus
                {
                    Name = region,
                    Status = status,
                    LastTrained = lastTrained,
                    ExerciseCount = exerciseCount
                });
            }

            return statuses;
        }

        public async Task<List<RecentWorkout>> GetRecentWorkoutsAsync(string userId, int count = 5)
        {
            var sessions = await _context.WorkoutSessions
                .Include(s => s.ExerciseLogs)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CompletedAt)
                .Take(count)
                .ToListAsync();

            return sessions.Select(s => new RecentWorkout
            {
                Id = s.Id,
                CompletedAt = s.CompletedAt,
                TimeAgo = FormatTimeAgo(s.CompletedAt),
                DurationMinutes = (int)Math.Round(s.DurationSeconds / 60.0),
                ExerciseCount = s.ExerciseLogs.Count,
                IntensityScore = s.IntensityScore,
                IntensityLabel = GetIntensityLabel(s.IntensityScore),
                MuscleGroups = s.ExerciseLogs.Select(l => l.MuscleGroup).Distinct().ToList()
            }).ToList();
        }

        public async Task<UserProfileStats> GetUserProfileStatsAsync(string userId)
        {
            var logs = await _context.WorkoutExerciseLogs
                .Include(l => l.WorkoutSession)
                .Where(l => l.WorkoutSession.UserId == userId)
                .ToListAsync();

            var muscleCounts = logs
                .GroupBy(l => l.MuscleGroup)
                .Select(g => new MuscleTrainingStat
                {
                    MuscleGroup = char.ToUpper(g.Key[0]) + g.Key.Substring(1).ToLower(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var mostTrained = muscleCounts.FirstOrDefault()?.MuscleGroup ?? "None yet";

            var totalSessions = await _context.WorkoutSessions.CountAsync(w => w.UserId == userId);
            var streak = await CalculateStreakAsync(userId);
            var totalExercises = logs.Count;

            var achievements = new List<Achievement>
            {
                new Achievement { Name = "First Blood", Description = "Complete your first workout session.", Icon = "Trophy", IsUnlocked = totalSessions >= 1 },
                new Achievement { Name = "Consistency is Key", Description = "Reach a 7-day workout streak.", Icon = "Flame", IsUnlocked = streak >= 7 },
                new Achievement { Name = "Century Club", Description = "Complete 100 workouts.", Icon = "Star", IsUnlocked = totalSessions >= 100 },
                new Achievement { Name = "Heavy Lifter", Description = "Log over 50 individual exercises.", Icon = "Medal", IsUnlocked = totalExercises >= 50 }
            };

            return new UserProfileStats
            {
                MostTrainedMuscle = mostTrained,
                MuscleTrainingCounts = muscleCounts,
                Achievements = achievements
            };
        }

        private static string FormatTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 60) return "Just now";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 1.5) return "Yesterday";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
            return $"{(int)(diff.TotalDays / 7)} weeks ago";
        }

        private static string GetIntensityLabel(int score)
        {
            if (score <= 25) return "Light";
            if (score <= 50) return "Moderate";
            if (score <= 75) return "High";
            return "Extreme";
        }
    }
}