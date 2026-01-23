using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services; // Add this using for the service

using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;
using System.Threading.Tasks;

namespace Final_project___MaxFitness.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMuscleService _muscleService; // Add the service field

        // Inject the service through the constructor
        public HomeController(ILogger<HomeController> logger, IMuscleService muscleService)
        {
            _logger = logger;
            _muscleService = muscleService;
        }

        public async Task<IActionResult> Index()
        {
            await Task.Delay(10);
            return View();
        }

        // --- ASYNC ACTIONS FOR EACH BODY PART ---

        public async Task<IActionResult> Chest() => await GetMuscleView("Chest");

        public async Task<IActionResult> Back() => await GetMuscleView("Back");

        public async Task<IActionResult> Legs() => await GetMuscleView("Legs");

        public async Task<IActionResult> Arms() => await GetMuscleView("Arms");

        // Generic private async method using the new models and service
        private async Task<IActionResult> GetMuscleView(string muscleName)
        {
            _logger.LogInformation($"Loading {muscleName} Details Page using master template.");

            // Use the service to fetch data asynchronously using your new models
            var statsTask = _muscleService.GetMuscleStatsAsync(muscleName);
            var exerciseTask = _muscleService.GetExercisesAsync(muscleName);

            // Execute tasks in parallel for performance
            await Task.WhenAll(statsTask, exerciseTask);

            // Pass the strongly-typed models to the view via ViewBag
            // In a real app, you might prefer a ViewModel, but this fits your current setup.
            ViewBag.Muscle = await statsTask;      // This is a MuscleProgress model
            ViewBag.Exercises = await exerciseTask; // This is a List<ExerciseDetail> model
            ViewBag.MuscleName = muscleName;

            // Continues to use your master template "Chest.cshtml"
            return View("Chest");
        }

        public async Task<IActionResult> Privacy()
        {
            await Task.Yield();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            return View(model);
        }
    }
}