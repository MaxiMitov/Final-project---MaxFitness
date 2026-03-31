using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Final_project___MaxFitness.Models
{
    public class PostComment
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual CommunityPost? Post { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PostLike
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual CommunityPost? Post { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
