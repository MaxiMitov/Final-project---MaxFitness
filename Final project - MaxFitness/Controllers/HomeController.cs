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

        public IActionResult Leaderboard()
        {
            var currentUser = User.Identity?.Name ?? "User";
            ViewBag.CurrentUser = currentUser;

            var leaders = new List<object>
            {
                new { Rank = 1, UserName = "FitBeast99", TotalWorkouts = 247, Streak = 34, TotalVolume = 128450.0, CaloriesBurned = 89200, TopMuscle = "Chest" },
                new { Rank = 2, UserName = "IronWolf", TotalWorkouts = 231, Streak = 28, TotalVolume = 115800.0, CaloriesBurned = 81500, TopMuscle = "Back" },
                new { Rank = 3, UserName = "GymSharK", TotalWorkouts = 198, Streak = 21, TotalVolume = 98200.0, CaloriesBurned = 72400, TopMuscle = "Legs" },
                new { Rank = 4, UserName = currentUser, TotalWorkouts = 42, Streak = 5, TotalVolume = 24500.0, CaloriesBurned = 18900, TopMuscle = "Chest" },
                new { Rank = 5, UserName = "LiftQueen", TotalWorkouts = 175, Streak = 14, TotalVolume = 87600.0, CaloriesBurned = 64300, TopMuscle = "Shoulders" },
                new { Rank = 6, UserName = "BeastMode_X", TotalWorkouts = 163, Streak = 11, TotalVolume = 79400.0, CaloriesBurned = 58700, TopMuscle = "Arms" },
                new { Rank = 7, UserName = "RepKing", TotalWorkouts = 148, Streak = 9, TotalVolume = 71200.0, CaloriesBurned = 52100, TopMuscle = "Back" },
                new { Rank = 8, UserName = "SwolePatrol", TotalWorkouts = 134, Streak = 7, TotalVolume = 65800.0, CaloriesBurned = 47800, TopMuscle = "Legs" },
                new { Rank = 9, UserName = "PumpMaster", TotalWorkouts = 121, Streak = 4, TotalVolume = 58900.0, CaloriesBurned = 41200, TopMuscle = "Chest" },
                new { Rank = 10, UserName = "GainzFactory", TotalWorkouts = 108, Streak = 3, TotalVolume = 52100.0, CaloriesBurned = 36500, TopMuscle = "Shoulders" }
            };

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