using Final_project___MaxFitness.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Final_project___MaxFitness.Services
{
    public class MuscleService : IMuscleService
    {
        public async Task<MuscleProgress> GetMuscleStatsAsync(string muscleName)
        {
            await Task.Delay(100); // Simulate DB Latency

            // Logic to provide unique PRs for each muscle group
            string pr = muscleName.ToLower() switch
            {
                "legs" => "315 lbs",
                "back" => "275 lbs",
                "chest" => "225 lbs",
                "arms" => "95 lbs",
                _ => "100 lbs"
            };

            return new MuscleProgress
            {
                Name = muscleName,
                CurrentPR = pr,
                ProgressPercentage = 85,
                Icon = muscleName == "Legs" ? "🦵" : "⚡"
            };
        }

        public async Task<List<ExerciseDetail>> GetExercisesAsync(string muscleName)
        {
            await Task.Delay(100); // Simulate DB Latency

            return muscleName.ToLower() switch
            {
                "chest" => new List<ExerciseDetail> {
                    new ExerciseDetail { Name = "Bench Press", TargetReps = "3 x 10", Intensity = "80% 1RM", Difficulty = "Intermediate" },
                    new ExerciseDetail { Name = "Dips", TargetReps = "3 x 12", Intensity = "Bodyweight", Difficulty = "Advanced" },
                    new ExerciseDetail { Name = "Push-ups", TargetReps = "4 x 20", Intensity = "Bodyweight", Difficulty = "Beginner" }
                },
                "back" => new List<ExerciseDetail> {
                    new ExerciseDetail { Name = "Deadlifts", TargetReps = "5 x 5", Intensity = "85% 1RM", Difficulty = "Advanced" },
                    new ExerciseDetail { Name = "Pull-ups", TargetReps = "3 x Max", Intensity = "Bodyweight", Difficulty = "Intermediate" }
                },
                "legs" => new List<ExerciseDetail> {
                    new ExerciseDetail { Name = "Squats", TargetReps = "4 x 8", Intensity = "75% 1RM", Difficulty = "Intermediate" },
                    new ExerciseDetail { Name = "Leg Press", TargetReps = "3 x 15", Intensity = "70% 1RM", Difficulty = "Beginner" }
                },
                "arms" => new List<ExerciseDetail> {
                    new ExerciseDetail { Name = "Bicep Curls", TargetReps = "3 x 12", Intensity = "Moderate", Difficulty = "Beginner" },
                    new ExerciseDetail { Name = "Tricep Dips", TargetReps = "3 x 15", Intensity = "Bodyweight", Difficulty = "Beginner" }
                },
                _ => new List<ExerciseDetail> {
                    new ExerciseDetail { Name = "Standard Movement", TargetReps = "3 x 10", Intensity = "N/A", Difficulty = "Beginner" }
                }
            };
        }
    }
}