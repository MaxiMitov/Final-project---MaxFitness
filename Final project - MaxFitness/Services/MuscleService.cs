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
                .ToListAsync();

            var totalVolume = logs.Sum(l => l.TotalVolume);
            var maxVolume = logs.Count > 0 ? logs.Max(l => l.TotalVolume) : 0;
            var totalSessions = logs.Select(l => l.WorkoutSessionId).Distinct().Count();

            var pr = maxVolume > 0 ? $"{maxVolume:F0} kg" : "No data";

            var progressPct = totalSessions > 0 ? Math.Min(100, totalSessions * 10) : 0;

            return new MuscleProgress
            {
                Name = muscleName,
                CurrentPR = pr,
                ProgressPercentage = progressPct,
                Icon = muscleName.ToLower() == "legs" ? "🦵" : "💪"
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
