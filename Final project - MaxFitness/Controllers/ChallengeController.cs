using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_project___MaxFitness.Controllers
{
    [Authorize]
    public class ChallengeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ChallengeController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var challenges = await _context.Challenges
                .Include(c => c.Participants)
                .OrderByDescending(c => c.ParticipantCount)
                .ToListAsync();

            var joinedIds = challenges
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Select(c => c.Id)
                .ToHashSet();

            ViewBag.Challenges = challenges;
            ViewBag.JoinedIds = joinedIds;
            ViewBag.CurrentUser = User.Identity?.Name ?? "User";

            return View();
        }

        public async Task<IActionResult> Detail(int id)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var currentUser = User.Identity?.Name ?? "User";

            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null) return RedirectToAction("Index");

            ViewBag.CurrentUser = currentUser;
            ViewBag.IsJoined = challenge.Participants.Any(p => p.UserId == userId);
            ViewBag.Challenge = new
            {
                challenge.Id, challenge.Name, challenge.Description, challenge.Icon, challenge.Color,
                challenge.DurationDays, challenge.StartDate, challenge.EndDate,
                challenge.Rules, challenge.Difficulty, challenge.ParticipantCount
            };
            ViewBag.Participants = challenge.Participants.Select(p => (object)new
            {
                UserName = p.User?.UserName ?? "Unknown",
                JoinedAt = p.JoinedAt.ToString("MMM d, yyyy"),
                Progress = p.Progress
            }).ToList();

            return View("~/Views/Home/ChallengeDetail.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Join([FromBody] ChallengeIdRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var challenge = await _context.Challenges.FindAsync(request.ChallengeId);
            if (challenge == null) return NotFound();

            var existing = await _context.ChallengeParticipants
                .FirstOrDefaultAsync(p => p.ChallengeId == request.ChallengeId && p.UserId == userId);

            if (existing == null)
            {
                _context.ChallengeParticipants.Add(new ChallengeParticipant
                {
                    ChallengeId = request.ChallengeId,
                    UserId = userId
                });
                challenge.ParticipantCount++;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Leave([FromBody] ChallengeIdRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var challenge = await _context.Challenges.FindAsync(request.ChallengeId);
            if (challenge == null) return NotFound();

            var existing = await _context.ChallengeParticipants
                .FirstOrDefaultAsync(p => p.ChallengeId == request.ChallengeId && p.UserId == userId);

            if (existing != null)
            {
                _context.ChallengeParticipants.Remove(existing);
                challenge.ParticipantCount = Math.Max(0, challenge.ParticipantCount - 1);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
    }
}
