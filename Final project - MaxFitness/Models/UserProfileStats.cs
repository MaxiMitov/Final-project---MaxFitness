namespace Final_project___MaxFitness.Models
{
    public class UserProfileStats
    {
        public string MostTrainedMuscle { get; set; } = string.Empty;
        public List<MuscleTrainingStat> MuscleTrainingCounts { get; set; } = new();
        public List<Achievement> Achievements { get; set; } = new();
    }

    public class MuscleTrainingStat
    {
        public string MuscleGroup { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class Achievement
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
    }
}