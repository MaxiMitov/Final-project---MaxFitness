using Final_project___MaxFitness.Models;

namespace Final_project___MaxFitness.Services
{
    public interface IWorkoutStatsService
    {
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<List<MuscleStatus>> GetMuscleStatusesAsync();
        Task<List<RecentWorkout>> GetRecentWorkoutsAsync(int count = 5);
    }
}
