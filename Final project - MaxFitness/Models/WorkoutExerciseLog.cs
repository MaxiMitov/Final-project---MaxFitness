using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_project___MaxFitness.Models
{
    public class WorkoutExerciseLog
    {
        [Key]
        public int Id { get; set; }

        public int WorkoutSessionId { get; set; }

        [ForeignKey("WorkoutSessionId")]
        public WorkoutSession WorkoutSession { get; set; } = null!;

        public string ExerciseName { get; set; } = string.Empty;

        public string MuscleGroup { get; set; } = string.Empty;

        public string SetsJson { get; set; } = "[]";

        public int SetCount { get; set; }

        public int TotalReps { get; set; }

        public double TotalVolume { get; set; }
    }
}
