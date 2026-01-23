using Final_project___MaxFitness.Models;

using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;
using System.Threading.Tasks;

namespace Final_project___MaxFitness.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Converted Index to Async
        public async Task<IActionResult> Index()
        {
            await Task.Delay(10); // Simulating minor async overhead
            return View();
        }

        // New Async Action for Chest Details
        public async Task<IActionResult> Chest()
        {
            _logger.LogInformation("Loading Chest Details Page");

            // Simulating multiple async data fetches (e.g., Exercises, PRs, History)
            var exerciseTask = FetchChestExercisesAsync();
            var statsTask = FetchChestStatsAsync();

            // Run tasks concurrently for efficiency
            await Task.WhenAll(exerciseTask, statsTask);

            ViewBag.Exercises = await exerciseTask;
            ViewBag.Stats = await statsTask;

            return View();
        }

        private async Task<List<string>> FetchChestExercisesAsync()
        {
            await Task.Delay(50); // Simulate DB Query
            return new List<string> { "Bench Press", "Incline Dumbbell Fly", "Cable Crossover", "Push-ups" };
        }

        private async Task<Dictionary<string, string>> FetchChestStatsAsync()
        {
            await Task.Delay(50); // Simulate DB Query
            return new Dictionary<string, string> {
                { "Max Rep", "225 lbs" },
                { "Volume This Week", "12,400 lbs" },
                { "Last Trained", "2 Days Ago" }
            };
        }

        public async Task<IActionResult> Privacy()
        {
            await Task.Yield();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}