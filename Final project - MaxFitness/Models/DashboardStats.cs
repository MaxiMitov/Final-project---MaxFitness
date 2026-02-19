namespace Final_project___MaxFitness.Models
{
    public class DashboardStats
    {
        public int WeeklyWorkouts { get; set; }
        public int WeeklyGoal { get; set; } = 6;
        public int WeeklyProgressPercent { get; set; }
        public int Streak { get; set; }
        public int CaloriesThisWeek { get; set; }
        public int AvgIntensity { get; set; }
    }

    public class MuscleStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "needs-work";
        public string LastTrained { get; set; } = "Never";
        public int ExerciseCount { get; set; }
    }

    public class RecentWorkout
    {
        public int Id { get; set; }
        public DateTime CompletedAt { get; set; }
        public string TimeAgo { get; set; } = "";
        public int DurationMinutes { get; set; }
        public int ExerciseCount { get; set; }
        public int IntensityScore { get; set; }
        public string IntensityLabel { get; set; } = "";
        public List<string> MuscleGroups { get; set; } = new();
    }
}
