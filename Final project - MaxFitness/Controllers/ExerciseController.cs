using Final_project___MaxFitness.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_project___MaxFitness.Controllers
{
    [Authorize]
    public class ExerciseController : Controller
    {
        private readonly AppDbContext _context;

        public ExerciseController(AppDbContext context)
        {
            _context = context;
        }

        // Browse exercise library
        public async Task<IActionResult> Index()
        {
            var exercises = await _context.Exercises
                .OrderBy(e => e.MuscleGroup)
                .ThenBy(e => e.Name)
                .ToListAsync();

            var grouped = exercises
                .GroupBy(e => e.MuscleGroup)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.ExerciseGroups = grouped;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetByMuscle(string muscleGroup)
        {
            var exercises = await _context.Exercises
                .Where(e => e.MuscleGroup.ToLower() == muscleGroup.ToLower())
                .ToListAsync();

            return Json(exercises.Select(e => new { e.Id, e.Name, e.MuscleGroup, e.Type }));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var results = await _context.Exercises
                .Where(e => e.Name.ToLower().Contains(query.ToLower()))
                .Take(10)
                .ToListAsync();

            return Json(results.Select(e => new { e.Id, e.Name, e.MuscleGroup, e.Type }));
        }
    }
}
