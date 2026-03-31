using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_project___MaxFitness.Models
{
    public class Challenge
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Challenge name is required.")]
        [StringLength(200, ErrorMessage = "Challenge name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        public string Icon { get; set; } = "fa-fire"; // FontAwesome icon class

        public string Color { get; set; } = "#e63946"; // Accent color

        public int DurationDays { get; set; } = 30;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime EndDate { get; set; }

        public string Rules { get; set; } = string.Empty; // Stored as semicolon-separated rules

        public string Difficulty { get; set; } = "Intermediate"; // Beginner, Intermediate, Advanced

        public int ParticipantCount { get; set; }

        public virtual ICollection<ChallengeParticipant> Participants { get; set; } = new List<ChallengeParticipant>();
    }

    public class ChallengeParticipant
    {
        [Key]
        public int Id { get; set; }

        public int ChallengeId { get; set; }

        [ForeignKey("ChallengeId")]
        public virtual Challenge? Challenge { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public int Progress { get; set; } // Days completed
    }
}
