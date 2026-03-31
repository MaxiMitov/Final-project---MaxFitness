using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Final_project___MaxFitness.Models
{
    public class CommunityPost
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required(ErrorMessage = "Post content cannot be empty.")]
        [StringLength(5000, ErrorMessage = "Post content cannot exceed 5000 characters.")]
        [MinLength(1, ErrorMessage = "Post content cannot be empty.")]
        public string Content { get; set; } = string.Empty;

        // Optional attached workout session
        public int? WorkoutSessionId { get; set; }

        [ForeignKey("WorkoutSessionId")]
        public virtual WorkoutSession? WorkoutSession { get; set; }

        // Optional photo URL (stored as base64 data URI or path)
        public string? PhotoUrl { get; set; }

        // Optional progress snapshot (e.g. "Streak: 7 days, Volume: 1200kg")
        public string? ProgressSnapshot { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Likes { get; set; }
        public int Comments { get; set; }
    }
}
