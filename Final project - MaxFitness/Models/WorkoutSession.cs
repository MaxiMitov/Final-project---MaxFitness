using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace Final_project___MaxFitness.Models
{
    public class WorkoutSession
    {
        [Key]
        public int Id { get; set; }

        // Link to the Identity User
        [Required(ErrorMessage = "User ID is required for a workout session.")]
        public string UserId { get; set; }

        public virtual IdentityUser? User { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public int DurationSeconds { get; set; }

        public int IntensityScore { get; set; }

        public int CaloriesBurned { get; set; }

        public double TotalVolume { get; set; }

        public int TotalSets { get; set; }

        public int TotalReps { get; set; }

        public List<WorkoutExerciseLog> ExerciseLogs { get; set; } = new();
    }
}