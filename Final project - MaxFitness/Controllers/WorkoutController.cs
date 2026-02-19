using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Final_project___MaxFitness.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly AppDbContext _context;

        public WorkoutController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetExercises(string muscleGroup)
        {
            var exercises = await _context.Exercises
                .Where(e => e.MuscleGroup == muscleGroup.ToLower())
                .Select(e => new { e.Name, e.Type })
                .ToListAsync();

            return Json(exercises);
        }

        [HttpPost]
        public async Task<IActionResult> SaveWorkout([FromBody] SaveWorkoutRequest request)
        {
            if (request == null || request.Exercises.Count == 0)
                return BadRequest("No exercises provided.");

            // Estimate calories: volume-based + duration-based for bodyweight
            var durationMinutes = Math.Max(request.DurationSeconds / 60.0, 1);
            var volumeCalories = request.TotalVolume * 0.05;
            var durationCalories = Math.Floor(durationMinutes) * 5;
            var calories = (int)Math.Round(Math.Max(volumeCalories, durationCalories));

            var session = new WorkoutSession
            {
                CompletedAt = DateTime.UtcNow,
                DurationSeconds = request.DurationSeconds,
                IntensityScore = request.IntensityScore,
                CaloriesBurned = calories,
                TotalVolume = request.TotalVolume,
                TotalSets = request.TotalSets,
                TotalReps = request.TotalReps
            };

            foreach (var ex in request.Exercises)
            {
                var setsJson = JsonSerializer.Serialize(ex.Sets);
                var filledSets = ex.Sets.Where(s => int.TryParse(s.Reps, out var r) && r > 0).ToList();
                var setCount = filledSets.Count > 0 ? filledSets.Count : ex.Sets.Count;
                var totalReps = filledSets.Sum(s => int.TryParse(s.Reps, out var r) ? r : 0);
                var totalVol = filledSets.Sum(s =>
                {
                    var r = int.TryParse(s.Reps, out var reps) ? reps : 0;
                    var w = double.TryParse(s.Weight, out var weight) ? weight : 0;
                    return r * w;
                });

                session.ExerciseLogs.Add(new WorkoutExerciseLog
                {
                    ExerciseName = ex.Name,
                    MuscleGroup = ex.Muscle.ToLower(),
                    SetsJson = setsJson,
                    SetCount = setCount,
                    TotalReps = totalReps,
                    TotalVolume = totalVol
                });
            }

            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = session.Id });
        }
    }
}
