using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_project___MaxFitness.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            // Site statistics
            var totalUsers = await _userManager.Users.CountAsync();
            var totalWorkouts = await _context.WorkoutSessions.CountAsync();
            var totalPosts = await _context.CommunityPosts.CountAsync();
            var totalChallenges = await _context.Challenges.CountAsync();
            var totalExerciseLogs = await _context.WorkoutExerciseLogs.CountAsync();
            var totalComments = await _context.PostComments.CountAsync();
            var totalLikes = await _context.PostLikes.CountAsync();
            var totalVolume = await _context.WorkoutSessions.SumAsync(s => s.TotalVolume);
            var totalCalories = await _context.WorkoutSessions.SumAsync(s => s.CaloriesBurned);

            // Activity this week
            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + (int)DayOfWeek.Monday);
            var workoutsThisWeek = await _context.WorkoutSessions.CountAsync(s => s.CompletedAt >= weekStart);
            var postsThisWeek = await _context.CommunityPosts.CountAsync(p => p.CreatedAt >= weekStart);
            var newUsersThisWeek = 0; // Identity doesn't track creation date by default

            // Recent users
            var users = await _userManager.Users.OrderByDescending(u => u.Id).Take(20).ToListAsync();
            var userList = new List<object>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var workoutCount = await _context.WorkoutSessions.CountAsync(s => s.UserId == u.Id);
                var postCount = await _context.CommunityPosts.CountAsync(p => p.UserId == u.Id);
                userList.Add(new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    Roles = string.Join(", ", roles),
                    IsAdmin = roles.Contains("Admin"),
                    WorkoutCount = workoutCount,
                    PostCount = postCount
                });
            }

            // Recent posts
            var recentPosts = await _context.CommunityPosts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new
                {
                    p.Id,
                    UserName = p.User != null ? p.User.UserName : "Unknown",
                    p.Content,
                    p.CreatedAt,
                    p.Likes,
                    p.Comments,
                    HasWorkout = p.WorkoutSessionId != null,
                    HasPhoto = p.PhotoUrl != null && p.PhotoUrl != ""
                })
                .ToListAsync();

            // Challenges
            var challengeList = await _context.Challenges
                .OrderByDescending(c => c.ParticipantCount)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.ParticipantCount,
                    c.Difficulty,
                    c.DurationDays,
                    c.Color
                })
                .ToListAsync();

            // Top muscle groups trained
            var muscleStats = await _context.WorkoutExerciseLogs
                .GroupBy(l => l.MuscleGroup)
                .Select(g => new { Muscle = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(8)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalWorkouts = totalWorkouts;
            ViewBag.TotalPosts = totalPosts;
            ViewBag.TotalChallenges = totalChallenges;
            ViewBag.TotalExerciseLogs = totalExerciseLogs;
            ViewBag.TotalComments = totalComments;
            ViewBag.TotalLikes = totalLikes;
            ViewBag.TotalVolume = totalVolume;
            ViewBag.TotalCalories = totalCalories;
            ViewBag.WorkoutsThisWeek = workoutsThisWeek;
            ViewBag.PostsThisWeek = postsThisWeek;
            ViewBag.Users = userList;
            ViewBag.RecentPosts = recentPosts;
            ViewBag.Challenges = challengeList;
            ViewBag.MuscleStats = muscleStats;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] IdRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null) return NotFound();
            if (await _userManager.IsInRoleAsync(user, "Admin")) return BadRequest("Cannot delete admin");

            // Delete user's data
            var sessions = _context.WorkoutSessions.Where(s => s.UserId == user.Id);
            var posts = _context.CommunityPosts.Where(p => p.UserId == user.Id);
            var comments = _context.PostComments.Where(c => c.UserId == user.Id);
            var likes = _context.PostLikes.Where(l => l.UserId == user.Id);
            var participants = _context.ChallengeParticipants.Where(p => p.UserId == user.Id);

            _context.PostComments.RemoveRange(comments);
            _context.PostLikes.RemoveRange(likes);
            _context.CommunityPosts.RemoveRange(posts);
            _context.ChallengeParticipants.RemoveRange(participants);
            _context.WorkoutSessions.RemoveRange(sessions);
            await _context.SaveChangesAsync();

            await _userManager.DeleteAsync(user);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin([FromBody] IdRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                return Json(new { success = true, isAdmin = false });
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Json(new { success = true, isAdmin = true });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost([FromBody] IntIdRequest request)
        {
            var post = await _context.CommunityPosts.FindAsync(request.Id);
            if (post == null) return NotFound();

            var comments = _context.PostComments.Where(c => c.PostId == post.Id);
            var likes = _context.PostLikes.Where(l => l.PostId == post.Id);
            _context.PostComments.RemoveRange(comments);
            _context.PostLikes.RemoveRange(likes);
            _context.CommunityPosts.Remove(post);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChallenge([FromBody] IntIdRequest request)
        {
            var challenge = await _context.Challenges.FindAsync(request.Id);
            if (challenge == null) return NotFound();

            var participants = _context.ChallengeParticipants.Where(p => p.ChallengeId == challenge.Id);
            _context.ChallengeParticipants.RemoveRange(participants);
            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeRequest request)
        {
            var challenge = new Challenge
            {
                Name = request.Name ?? "New Challenge",
                Description = request.Description ?? "",
                Icon = request.Icon ?? "fa-fire",
                Color = request.Color ?? "#e63946",
                DurationDays = request.DurationDays > 0 ? request.DurationDays : 30,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(request.DurationDays > 0 ? request.DurationDays : 30),
                Rules = request.Rules ?? "",
                Difficulty = request.Difficulty ?? "Intermediate"
            };

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = challenge.Id });
        }
    }

    // Request models for admin actions
    public class IdRequest { public string Id { get; set; } = ""; }
    public class IntIdRequest { public int Id { get; set; } }
    public class CreateChallengeRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int DurationDays { get; set; }
        public string? Rules { get; set; }
        public string? Difficulty { get; set; }
    }
}
