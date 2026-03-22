using Final_project___MaxFitness.Models;

namespace Final_project___MaxFitness.Services
{
    public interface IWorkoutStatsService
    {
        Task<DashboardStats> GetDashboardStatsAsync(string userId);
        Task<List<MuscleStatus>> GetMuscleStatusesAsync(string userId);
        Task<List<RecentWorkout>> GetRecentWorkoutsAsync(string userId, int count = 5);
    }
}