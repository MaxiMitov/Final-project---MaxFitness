using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;

using Microsoft.EntityFrameworkCore;

namespace Final_project___MaxFitness.Services
{
    public class MuscleService : IMuscleService
    {
        private readonly AppDbContext _context;

        private static readonly Dictionary<string, string[]> MuscleGroupMapping = new()
        {
            ["chest"] = new[] { "chest" },
            ["back"] = new[] { "back" },
            ["shoulders"] = new[] { "shoulders" },
            ["biceps"] = new[] { "biceps" },
            ["triceps"] = new[] { "triceps" },
            ["arms"] = new[] { "biceps", "triceps" },
            ["legs"] = new[] { "legs" },
            ["abs"] = new[] { "abs" },
            ["forearms"] = new[] { "forearms" }
        };

        public MuscleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MuscleProgress> GetMuscleStatsAsync(string muscleName, string userId)
        {
            var key = muscleName.ToLower();
            var dbGroups = MuscleGroupMapping.GetValueOrDefault(key) ?? new[] { key };

            var logs = await _context.WorkoutExerciseLogs
                .Include(l => l.WorkoutSession)
                .Where(l => l.WorkoutSession.UserId == userId && dbGroups.Contains(l.MuscleGroup.ToLower()))
                .OrderByDescending(l => l.WorkoutSession.CompletedAt)
                .ToListAsync();

            // Current PR - max volume in a single exercise log
            var maxVolume = logs.Count > 0 ? logs.Max(l => l.TotalVolume) : 0;
            var pr = maxVolume > 0 ? $"{maxVolume:F0} kg" : "No data";

            // Total unique sessions for this muscle
            var totalSessions = logs.Select(l => l.WorkoutSessionId).Distinct().Count();
            var progressPct = totalSessions > 0 ? Math.Min(100, totalSessions * 10) : 0;

            // Weekly sets - sets done this week
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            if (now.DayOfWeek == DayOfWeek.Sunday)
                startOfWeek = startOfWeek.AddDays(-7);

            var weeklySets = logs
                .Where(l => l.WorkoutSession.CompletedAt >= startOfWeek)
                .Sum(l => l.SetCount);

            // Recovery status based on last trained date
            var lastSession = logs.FirstOrDefault()?.WorkoutSession.CompletedAt;
            string recoveryStatus;
            string nextSession;
            if (lastSession == null)
            {
                recoveryStatus = "Ready";
                nextSession = "Anytime";
            }
            else
            {
                var hoursSince = (now - lastSession.Value).TotalHours;
                if (hoursSince < 24)
                {
                    recoveryStatus = "Recovering";
                    nextSession = "In 24h";
                }
                else if (hoursSince < 48)
                {
                    recoveryStatus = "Almost Ready";
                    nextSession = "Tomorrow";
                }
                else
                {
                    recoveryStatus = "Optimal";
                    nextSession = "Today";
                }
            }

            // Exercise counts from DB
            var exercises = await _context.Exercises
                .Where(e => dbGroups.Contains(e.MuscleGroup.ToLower()))
                .ToListAsync();

            var compoundCount = exercises.Count(e => e.Type == "Compound");
            var isolationCount = exercises.Count(e => e.Type == "Isolation");
            var bodyweightCount = exercises.Count(e => e.Type == "Bodyweight");

            // Personal records - top volumes by month
            var prRecords = logs
                .GroupBy(l => new { l.WorkoutSession.CompletedAt.Year, l.WorkoutSession.CompletedAt.Month })
                .Select(g => new
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    MaxVolume = g.Max(l => l.TotalVolume)
                })
                .OrderByDescending(x => x.Date)
                .Take(5)
                .Select(x => new PersonalRecord
                {
                    Weight = $"{x.MaxVolume:F0} kg",
                    Date = x.Date.ToString("MMM yyyy")
                })
                .ToList();

            // Monthly progress - sets per week for last 4 weeks
            var weeklyVolumes = new List<WeeklyVolume>();
            for (int i = 3; i >= 0; i--)
            {
                var weekStart = startOfWeek.AddDays(-7 * i);
                var weekEnd = weekStart.AddDays(7);
                var sets = logs
                    .Where(l => l.WorkoutSession.CompletedAt >= weekStart && l.WorkoutSession.CompletedAt < weekEnd)
                    .Sum(l => l.SetCount);
                weeklyVolumes.Add(new WeeklyVolume
                {
                    Label = $"Week {4 - i}",
                    Sets = sets
                });
            }

            var maxSets = weeklyVolumes.Max(w => w.Sets);
            if (maxSets == 0) maxSets = 1;
            foreach (var wv in weeklyVolumes)
            {
                wv.HeightPercent = maxSets > 0 ? (int)Math.Round(wv.Sets * 100.0 / maxSets) : 0;
                if (wv.HeightPercent < 4 && wv.Sets > 0) wv.HeightPercent = 4;
            }

            // Trend calculation
            var firstWeekSets = weeklyVolumes.FirstOrDefault()?.Sets ?? 0;
            var lastWeekSets = weeklyVolumes.LastOrDefault()?.Sets ?? 0;
            var trendPct = firstWeekSets > 0
                ? ((lastWeekSets - firstWeekSets) * 100.0 / firstWeekSets).ToString("+0;-0;0")
                : lastWeekSets > 0 ? "+100" : "0";

            return new MuscleProgress
            {
                Name = muscleName,
                CurrentPR = pr,
                ProgressPercentage = progressPct,
                Icon = muscleName.ToLower() == "legs" ? "🦵" : "💪",
                WeeklySets = weeklySets,
                RecoveryStatus = recoveryStatus,
                NextSession = nextSession,
                TotalExercises = exercises.Count,
                CompoundCount = compoundCount,
                IsolationCount = isolationCount,
                BodyweightCount = bodyweightCount,
                PersonalRecords = prRecords,
                MonthlyProgress = weeklyVolumes,
                TrendPercent = trendPct
            };
        }

        public async Task<List<ExerciseDetail>> GetExercisesAsync(string muscleName)
        {
            var key = muscleName.ToLower();
            var dbGroups = MuscleGroupMapping.GetValueOrDefault(key) ?? new[] { key };

            var exercises = await _context.Exercises
                .Where(e => dbGroups.Contains(e.MuscleGroup.ToLower()))
                .ToListAsync();

            return exercises.Select(e => new ExerciseDetail
            {
                Name = e.Name,
                TargetReps = e.Type == "Compound" ? "4 x 8" : e.Type == "Bodyweight" ? "3 x 15" : "3 x 12",
                Intensity = e.Type == "Compound" ? "75% 1RM" : e.Type == "Bodyweight" ? "Bodyweight" : "Moderate",
                Difficulty = e.Type == "Compound" ? "Intermediate" : e.Type == "Bodyweight" ? "Beginner" : "Beginner"
            }).ToList();
        }
    }
}
