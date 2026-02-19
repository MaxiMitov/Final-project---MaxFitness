using Microsoft.AspNetCore.Mvc;

namespace Final_project___MaxFitness.Controllers
{
    public class WorkoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
