using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        public HomeController(ILogger<HomeController> logger, IMuscleService muscleService, IWorkoutStatsService workoutStatsService, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _muscleService = muscleService;
            _workoutStatsService = workoutStatsService;
            _userManager = userManager;
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