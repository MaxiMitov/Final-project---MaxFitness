namespace Final_project___MaxFitness.Models
{
    public class MuscleProgress
    {
        public string Name { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public string CurrentPR { get; set; } = "0 lbs";
        public string Icon { get; set; } = "⚡";
    }
}