namespace Final_project___MaxFitness.Models
{
    public class SaveWorkoutRequest
    {
        public int DurationSeconds { get; set; }
        public int IntensityScore { get; set; }
        public int TotalSets { get; set; }
        public int TotalReps { get; set; }
        public double TotalVolume { get; set; }
        public List<WorkoutExerciseDto> Exercises { get; set; } = new();
    }

    public class WorkoutExerciseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Muscle { get; set; } = string.Empty;
        public List<SetDto> Sets { get; set; } = new();
    }

    public class SetDto
    {
        public string Reps { get; set; } = "";
        public string Weight { get; set; } = "";
    }
}
