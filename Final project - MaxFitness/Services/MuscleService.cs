using Final_project___MaxFitness.Models;

namespace Final_project___MaxFitness.Services
{
    public class MuscleService : IMuscleService
    {
        public async Task<MuscleProgress> GetMuscleStatsAsync(string muscleName)
        {
            await Task.Delay(100); // Simulate DB Latency
            return new MuscleProgress
            {
                Name = muscleName,
                CurrentPR = muscleName == "Legs" ? "315 lbs" : "225 lbs",
                ProgressPercentage = 85,
                Icon = muscleName == "Legs" ? "🦵" : "⚡"
            };
        }

        public async Task<List<ExerciseDetail>> GetExercisesAsync(string muscleName)
        {
            await Task.Delay(100);
            return muscleName.ToLower() switch
            {
                "chest" => new List<ExerciseDetail> {
                    new ExerciseDetail { Name = "Bench Press" },
                    new ExerciseDetail { Name = "Dips" }
                },
                "legs" => new List<ExerciseDetail> {
                    new ExerciseDetail { Name = "Squats" },
                    new ExerciseDetail { Name = "Leg Press" }
                },
                _ => new List<ExerciseDetail> { new ExerciseDetail { Name = "Standard Movement" } }
            };
        }
    }
}