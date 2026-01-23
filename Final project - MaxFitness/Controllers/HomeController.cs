using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services;

using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;
using System.Threading.Tasks;

namespace Final_project___MaxFitness.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMuscleService _muscleService;

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

        private async Task<IActionResult> GetMuscleView(string muscleName)
        {
            _logger.LogInformation($"Loading {muscleName} Details Page.");

            // FETCH DATA ASYNCHRONOUSLY
            var statsTask = _muscleService.GetMuscleStatsAsync(muscleName);
            var exerciseTask = _muscleService.GetExercisesAsync(muscleName);

            // RUN BOTH TASKS AT ONCE
            await Task.WhenAll(statsTask, exerciseTask);

            // ASSIGN TO VIEW BAG
            ViewBag.MuscleName = muscleName;
            ViewBag.Muscle = await statsTask;
            ViewBag.Exercises = await exerciseTask;

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