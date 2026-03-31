using System.ComponentModel.DataAnnotations;

namespace Final_project___MaxFitness.Models
{
    public class Exercise
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Exercise name is required.")]
        [StringLength(100, ErrorMessage = "Exercise name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Muscle group is required.")]
        public string MuscleGroup { get; set; } = string.Empty;

        public string Type { get; set; } = "Compound";
    }
}
