using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;
using Microsoft.EntityFrameworkCore;

namespace Final_project___MaxFitness.Services
{
    public class WorkoutStatsService : IWorkoutStatsService
    {
        private readonly AppDbContext _context;

        // Maps workout muscle groups to body map region names
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

        // All body map regions
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

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            if (now.DayOfWeek == DayOfWeek.Sunday)
                startOfWeek = startOfWeek.AddDays(-7);

            var weekWorkouts = await _context.WorkoutSessions
                .Where(w => w.CompletedAt >= startOfWeek)
                .ToListAsync();

            var weeklyCount = weekWorkouts.Count;
            var goal = 6;
            var progressPercent = Math.Min(100, (int)Math.Round((weeklyCount / (double)goal) * 100));
            var caloriesThisWeek = weekWorkouts.Sum(w => w.CaloriesBurned);

            // Average intensity of recent workouts (last 10)
            var recentIntensities = await _context.WorkoutSessions
                .OrderByDescending(w => w.CompletedAt)
                .Take(10)
                .Select(w => w.IntensityScore)
                .ToListAsync();

            var avgIntensity = recentIntensities.Count > 0
                ? (int)Math.Round(recentIntensities.Average())
                : 0;

            // Streak: consecutive days with at least one workout
            var streak = await CalculateStreakAsync();

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

        private async Task<int> CalculateStreakAsync()
        {
            var workoutDates = await _context.WorkoutSessions
                .OrderByDescending(w => w.CompletedAt)
                .Select(w => w.CompletedAt.Date)
                .ToListAsync();

            if (workoutDates.Count == 0) return 0;

            var uniqueDates = workoutDates.Distinct().OrderByDescending(d => d).ToList();
            var today = DateTime.UtcNow.Date;

            // Streak must include today or yesterday
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

        public async Task<List<MuscleStatus>> GetMuscleStatusesAsync()
        {
            var now = DateTime.UtcNow;

            // Get all exercise logs with their session dates
            var logs = await _context.WorkoutExerciseLogs
                .Include(l => l.WorkoutSession)
                .OrderByDescending(l => l.WorkoutSession.CompletedAt)
                .ToListAsync();

            // Build a map: muscle group -> most recent training date
            var lastTrainedByGroup = new Dictionary<string, DateTime>();
            foreach (var log in logs)
            {
                var group = log.MuscleGroup.ToLower();
                if (!lastTrainedByGroup.ContainsKey(group))
                    lastTrainedByGroup[group] = log.WorkoutSession.CompletedAt;
            }

            // Map each body region to its status
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

        public async Task<List<RecentWorkout>> GetRecentWorkoutsAsync(int count = 5)
        {
            var sessions = await _context.WorkoutSessions
                .Include(s => s.ExerciseLogs)
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
