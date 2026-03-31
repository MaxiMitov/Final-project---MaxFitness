namespace Final_project___MaxFitness.Models
{
    public class MuscleProgress
    {
        public string Name { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public string CurrentPR { get; set; } = "0 lbs";
        public string Icon { get; set; } = "⚡";
        public int WeeklySets { get; set; }
        public string RecoveryStatus { get; set; } = "Unknown";
        public string NextSession { get; set; } = "N/A";
        public int TotalExercises { get; set; }
        public int CompoundCount { get; set; }
        public int IsolationCount { get; set; }
        public int BodyweightCount { get; set; }
        public List<PersonalRecord> PersonalRecords { get; set; } = new();
        public List<WeeklyVolume> MonthlyProgress { get; set; } = new();
        public string TrendPercent { get; set; } = "0";
    }

    public class PersonalRecord
    {
        public string Weight { get; set; } = "";
        public string Date { get; set; } = "";
    }

    public class WeeklyVolume
    {
        public string Label { get; set; } = "";
        public int Sets { get; set; }
        public int HeightPercent { get; set; }
    }
}