using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services;

namespace Final_project___MaxFitness.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWorkoutStatsService _statsService;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            IWorkoutStatsService statsService)
        {
            _userManager = userManager;
            _statsService = statsService;
        }

        public string Username { get; set; } = string.Empty;
        public UserProfileStats Stats { get; set; } = new UserProfileStats();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            Username = user.UserName ?? string.Empty;
            Stats = await _statsService.GetUserProfileStatsAsync(user.Id);

            return Page();
        }
    }
}