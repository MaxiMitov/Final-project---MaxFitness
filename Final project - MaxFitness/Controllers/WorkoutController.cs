using Final_project___MaxFitness.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_project___MaxFitness.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly AppDbContext _context;

        public WorkoutController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetExercises(string muscleGroup)
        {
            var exercises = await _context.Exercises
                .Where(e => e.MuscleGroup == muscleGroup.ToLower())
                .Select(e => new { e.Name, e.Type })
                .ToListAsync();

            return Json(exercises);
        }
    }
}
