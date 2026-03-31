namespace Final_project___MaxFitness.Models
{
    public class CreatePostRequest
    {
        public string? Content { get; set; }
        public int? WorkoutSessionId { get; set; }
        public string? PhotoUrl { get; set; }
        public string? ProgressSnapshot { get; set; }
    }
}
