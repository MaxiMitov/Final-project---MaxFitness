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

        // Dashboard Home Page
        public async Task<IActionResult> Index()
        {
            await Task.Delay(10); // Simulate async work
            return View();
        }

        // --- ASYNC ACTIONS FOR EACH BODY PART ---
        // All of these now redirect to the same "Chest.cshtml" view but with different data

        public async Task<IActionResult> Chest() => await GetMuscleView("Chest");

        public async Task<IActionResult> Back() => await GetMuscleView("Back");

        public async Task<IActionResult> Legs() => await GetMuscleView("Legs");

        public async Task<IActionResult> Arms() => await GetMuscleView("Arms");


        // Generic private async method to handle data fetching
        private async Task<IActionResult> GetMuscleView(string muscleName)
        {
            _logger.LogInformation($"Loading {muscleName} Details Page using master template.");

            // Start multiple data fetches concurrently to satisfy async requirements and improve speed
            var exerciseTask = FetchExercisesAsync(muscleName);
            var statsTask = FetchStatsAsync(muscleName);

            // Wait for all tasks to complete in parallel
            await Task.WhenAll(exerciseTask, statsTask);

            // Pass the data to the view via ViewBag
            ViewBag.MuscleName = muscleName;
            ViewBag.Exercises = await exerciseTask;
            ViewBag.Stats = await statsTask;

            // This line tells ASP.NET to use the "Chest.cshtml" file 
            // no matter which action was called.
            return View("Chest");
        }

        private async Task<List<string>> FetchExercisesAsync(string muscle)
        {
            await Task.Delay(75); // Simulate slightly longer DB Query for async demonstration
            return muscle switch
            {
                "Chest" => new List<string> { "Bench Press", "Dumbbell Flys", "Push-ups", "Cable Crossovers" },
                "Back" => new List<string> { "Deadlifts", "Pull-ups", "Bent Over Rows", "Lat Pulldowns" },
                "Legs" => new List<string> { "Squats", "Leg Press", "Calf Raises", "Hamstring Curls" },
                "Arms" => new List<string> { "Bicep Curls", "Tricep Dips", "Hammer Curls", "Skull Crushers" },
                _ => new List<string> { "Generic Exercise" }
            };
        }

        private async Task<Dictionary<string, string>> FetchStatsAsync(string muscle)
        {
            await Task.Delay(75); // Simulate DB Query

            // Logic to provide different PRs based on the muscle selected
            string maxWeight = muscle switch
            {
                "Legs" => "315 lbs",
                "Back" => "275 lbs",
                "Chest" => "225 lbs",
                "Arms" => "95 lbs",
                _ => "0 lbs"
            };

            return new Dictionary<string, string> {
                { "Max Rep", maxWeight },
                { "Volume This Week", "14,200 lbs" },
                { "Last Trained", "Yesterday" }
            };
        }

        public async Task<IActionResult> Privacy()
        {
            await Task.Yield(); // Explicitly yield for async flow
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            // Simple check for Activity.Current using null-coalescing
            var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            return await Task.FromResult<IActionResult>(View(model));
        }
    }
}