using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace Final_project___MaxFitness.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMuscleService _muscleService;
        private readonly IWorkoutStatsService _workoutStatsService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, IMuscleService muscleService, IWorkoutStatsService workoutStatsService, UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _logger = logger;
            _muscleService = muscleService;
            _workoutStatsService = workoutStatsService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var statsTask = _workoutStatsService.GetDashboardStatsAsync(userId);
            var musclesTask = _workoutStatsService.GetMuscleStatusesAsync(userId);
            var recentTask = _workoutStatsService.GetRecentWorkoutsAsync(userId, 5);

            await Task.WhenAll(statsTask, musclesTask, recentTask);

            ViewBag.Stats = await statsTask;
            ViewBag.MuscleStatuses = await musclesTask;
            ViewBag.RecentWorkouts = await recentTask;

            return View();
        }

        public async Task<IActionResult> Chest() => await GetMuscleView("Chest");
        public async Task<IActionResult> Back() => await GetMuscleView("Back");
        public async Task<IActionResult> Legs() => await GetMuscleView("Legs");
        public async Task<IActionResult> Arms() => await GetMuscleView("Arms");
        public async Task<IActionResult> Shoulders() => await GetMuscleView("Shoulders");
        public async Task<IActionResult> Abs() => await GetMuscleView("Abs");
        public async Task<IActionResult> Forearms() => await GetMuscleView("Forearms");
        public async Task<IActionResult> Triceps() => await GetMuscleView("Triceps");
        public async Task<IActionResult> Biceps() => await GetMuscleView("Biceps");

        private async Task<IActionResult> GetMuscleView(string muscleName)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var statsTask = _muscleService.GetMuscleStatsAsync(muscleName, userId);
            var exerciseTask = _muscleService.GetExercisesAsync(muscleName);

            await Task.WhenAll(statsTask, exerciseTask);

            ViewBag.MuscleName = muscleName;
            ViewBag.Muscle = await statsTask;
            ViewBag.Exercises = await exerciseTask;

            return View("Chest");
        }

        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = user.Id;
            var profileStats = await _workoutStatsService.GetUserProfileStatsAsync(userId);

            ViewBag.UserName = user.UserName ?? "User";
            ViewBag.UserEmail = user.Email ?? "";
            ViewBag.JoinDate = "March 2026";
            ViewBag.TotalWorkouts = profileStats.MuscleTrainingCounts.Sum(m => m.Count);
            ViewBag.ProfileStats = profileStats;

            return View();
        }

        public async Task<IActionResult> Leaderboard()
        {
            var currentUser = User.Identity?.Name ?? "User";
            ViewBag.CurrentUser = currentUser;

            // Get all users with their workout data
            var userSessions = await _context.WorkoutSessions
                .Include(s => s.ExerciseLogs)
                .Include(s => s.User)
                .ToListAsync();

            // Group by user and calculate stats
            var userStats = userSessions
                .GroupBy(s => s.UserId)
                .Select(g =>
                {
                    var sessions = g.ToList();
                    var userName = sessions.First().User?.UserName ?? "Unknown";
                    var totalWorkouts = sessions.Count;
                    var totalVolume = sessions.Sum(s => s.TotalVolume);
                    var caloriesBurned = sessions.Sum(s => s.CaloriesBurned);

                    // Calculate streak
                    var uniqueDates = sessions
                        .Select(s => s.CompletedAt.Date)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .ToList();

                    var streak = 0;
                    if (uniqueDates.Count > 0)
                    {
                        var today = DateTime.UtcNow.Date;
                        if (uniqueDates[0] >= today.AddDays(-1))
                        {
                            streak = 1;
                            for (int i = 1; i < uniqueDates.Count; i++)
                            {
                                if ((uniqueDates[i - 1] - uniqueDates[i]).Days == 1)
                                    streak++;
                                else
                                    break;
                            }
                        }
                    }

                    // Find top muscle group
                    var topMuscle = sessions
                        .SelectMany(s => s.ExerciseLogs)
                        .GroupBy(l => l.MuscleGroup)
                        .OrderByDescending(mg => mg.Count())
                        .Select(mg => char.ToUpper(mg.Key[0]) + mg.Key.Substring(1))
                        .FirstOrDefault() ?? "N/A";

                    return new { UserName = userName, TotalWorkouts = totalWorkouts, Streak = streak, TotalVolume = totalVolume, CaloriesBurned = caloriesBurned, TopMuscle = topMuscle };
                })
                .OrderByDescending(u => u.TotalWorkouts)
                .ToList();

            // Assign ranks
            var leaders = userStats
                .Select((u, i) => (object)new { Rank = i + 1, u.UserName, u.TotalWorkouts, u.Streak, u.TotalVolume, u.CaloriesBurned, u.TopMuscle })
                .ToList();

            ViewBag.Leaders = leaders;
            return View();
        }

        public IActionResult Community()
        {
            var currentUser = User.Identity?.Name ?? "User";
            ViewBag.CurrentUser = currentUser;

            var posts = new List<object>
            {
                new { UserName = "FitBeast99", TimeAgo = "2h ago", Content = "Just crushed a new PR on bench press! 225 lbs for 3 reps. Feeling unstoppable today.", WorkoutName = "Push Day", Duration = 65, ExerciseCount = 6, Intensity = "High", Likes = 24, Comments = 8, MuscleGroups = new List<string> { "chest", "shoulders", "triceps" } },
                new { UserName = "IronWolf", TimeAgo = "4h ago", Content = "Back day is best day. Nothing beats the pump from heavy deadlifts.", WorkoutName = "Pull Day", Duration = 55, ExerciseCount = 5, Intensity = "Extreme", Likes = 18, Comments = 5, MuscleGroups = new List<string> { "back", "biceps" } },
                new { UserName = "GymSharK", TimeAgo = "6h ago", Content = "Leg day done. Can barely walk but it was worth it. Squats + lunges combo is brutal.", WorkoutName = "Leg Day", Duration = 70, ExerciseCount = 7, Intensity = "High", Likes = 31, Comments = 12, MuscleGroups = new List<string> { "legs" } },
                new { UserName = "LiftQueen", TimeAgo = "1d ago", Content = "7-day streak complete! Consistency is everything. Keep showing up.", WorkoutName = "", Duration = 0, ExerciseCount = 0, Intensity = "", Likes = 45, Comments = 15, MuscleGroups = new List<string>() },
                new { UserName = currentUser, TimeAgo = "1d ago", Content = "Started tracking my workouts with MaxFitness. Excited to see where this goes!", WorkoutName = "Full Body", Duration = 45, ExerciseCount = 4, Intensity = "Moderate", Likes = 12, Comments = 3, MuscleGroups = new List<string> { "chest", "back", "legs" } }
            };

            ViewBag.Posts = posts;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}