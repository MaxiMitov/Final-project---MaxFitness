using Final_project___MaxFitness.Models;

public interface IMuscleService
{
    Task<MuscleProgress> GetMuscleStatsAsync(string muscleName);
    Task<List<ExerciseDetail>> GetExercisesAsync(string muscleName);
}