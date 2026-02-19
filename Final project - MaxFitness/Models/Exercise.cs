using System.ComponentModel.DataAnnotations;

namespace Final_project___MaxFitness.Models
{
    public class Exercise
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string MuscleGroup { get; set; } = string.Empty;

        public string Type { get; set; } = "Compound";
    }
}
