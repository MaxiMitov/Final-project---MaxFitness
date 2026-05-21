using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace Final_project___MaxFitness.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMuscleService _muscleService;
        private readonly IWorkoutStatsService _workoutStatsService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, IMuscleService muscleService, IWorkoutStatsService workoutStatsService, UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _logger = logger;
            _muscleService = muscleService;
            _workoutStatsService = workoutStatsService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            ViewBag.Stats = await _workoutStatsService.GetDashboardStatsAsync(userId);
            ViewBag.MuscleStatuses = await _workoutStatsService.GetMuscleStatusesAsync(userId);
            ViewBag.RecentWorkouts = await _workoutStatsService.GetRecentWorkoutsAsync(userId, 5);

            return View();
        }

        public async Task<IActionResult> Chest() => await GetMuscleView("Chest");
        public async Task<IActionResult> Back() => await GetMuscleView("Back");
        public async Task<IActionResult> Legs() => await GetMuscleView("Legs");
        public async Task<IActionResult> Arms() => await GetMuscleView("Arms");
        public async Task<IActionResult> Shoulders() => await GetMuscleView("Shoulders");
        public async Task<IActionResult> Abs() => await GetMuscleView("Abs");
        public async Task<IActionResult> Forearms() => await GetMuscleView("Forearms");
        public async Task<IActionResult> Triceps() => await GetMuscleView("Triceps");
        public async Task<IActionResult> Biceps() => await GetMuscleView("Biceps");

        private async Task<IActionResult> GetMuscleView(string muscleName)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            ViewBag.MuscleName = muscleName;
            ViewBag.Muscle = await _muscleService.GetMuscleStatsAsync(muscleName, userId);
            ViewBag.Exercises = await _muscleService.GetExercisesAsync(muscleName);

            return View("Chest");
        }

        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = user.Id;
            var profileStats = await _workoutStatsService.GetUserProfileStatsAsync(userId);

            ViewBag.UserName = user.UserName ?? "User";
            ViewBag.UserEmail = user.Email ?? "";
            ViewBag.JoinDate = "March 2026";
            ViewBag.TotalWorkouts = profileStats.MuscleTrainingCounts.Sum(m => m.Count);
            ViewBag.ProfileStats = profileStats;

            return View();
        }

        public async Task<IActionResult> Leaderboard(string period = "all")
        {
            var currentUser = User.Identity?.Name ?? "User";
            ViewBag.CurrentUser = currentUser;
            ViewBag.Period = period;

            // Determine date filter
            var now = DateTime.UtcNow;
            DateTime? cutoff = period switch
            {
                "week" => now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday + (now.DayOfWeek == DayOfWeek.Sunday ? -7 : 0)),
                "month" => new DateTime(now.Year, now.Month, 1),
                _ => null
            };

            // Query sessions with optional date filter
            var query = _context.WorkoutSessions
                .Include(s => s.ExerciseLogs)
                .Include(s => s.User)
                .AsQueryable();

            if (cutoff.HasValue)
                query = query.Where(s => s.CompletedAt >= cutoff.Value);

            var userSessions = await query.ToListAsync();

            // Group by user and calculate stats, ranked by total volume
            var userStats = userSessions
                .GroupBy(s => s.UserId)
                .Select(g =>
                {
                    var sessions = g.ToList();
                    var userName = sessions.First().User?.UserName ?? "Unknown";
                    var totalWorkouts = sessions.Count;
                    var totalVolume = sessions.Sum(s => s.TotalVolume);
                    var caloriesBurned = sessions.Sum(s => s.CaloriesBurned);

                    // Calculate streak from ALL sessions (not filtered)
                    var uniqueDates = sessions
                        .Select(s => s.CompletedAt.Date)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .ToList();

                    var streak = 0;
                    if (uniqueDates.Count > 0)
                    {
                        var today = DateTime.UtcNow.Date;
                        if (uniqueDates[0] >= today.AddDays(-1))
                        {
                            streak = 1;
                            for (int i = 1; i < uniqueDates.Count; i++)
                            {
                                if ((uniqueDates[i - 1] - uniqueDates[i]).Days == 1)
                                    streak++;
                                else
                                    break;
                            }
                        }
                    }

                    var topMuscle = sessions
                        .SelectMany(s => s.ExerciseLogs)
                        .GroupBy(l => l.MuscleGroup)
                        .OrderByDescending(mg => mg.Count())
                        .Select(mg => char.ToUpper(mg.Key[0]) + mg.Key.Substring(1))
                        .FirstOrDefault() ?? "N/A";

                    return new { UserName = userName, TotalWorkouts = totalWorkouts, Streak = streak, TotalVolume = totalVolume, CaloriesBurned = caloriesBurned, TopMuscle = topMuscle };
                })
                .OrderByDescending(u => u.TotalVolume)
                .ToList();

            var leaders = userStats
                .Select((u, i) => (object)new { Rank = i + 1, u.UserName, u.TotalWorkouts, u.Streak, u.TotalVolume, u.CaloriesBurned, u.TopMuscle })
                .ToList();

            ViewBag.Leaders = leaders;
            return View();
        }

        public async Task<IActionResult> Community()
        {
            var currentUser = User.Identity?.Name ?? "User";
            var userId = _userManager.GetUserId(User) ?? "";
            ViewBag.CurrentUser = currentUser;

            // Load real posts from DB
            var dbPosts = await _context.CommunityPosts
                .Include(p => p.User)
                .Include(p => p.WorkoutSession)
                    .ThenInclude(ws => ws!.ExerciseLogs)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();

            // Get user's liked post IDs
            var likedPostIds = await _context.PostLikes
                .Where(l => l.UserId == userId)
                .Select(l => l.PostId)
                .ToListAsync();

            var posts = dbPosts.Select(p =>
            {
                var ws = p.WorkoutSession;
                var workoutName = "";
                var duration = 0;
                var exerciseCount = 0;
                var intensity = "";
                var muscleGroups = new List<string>();

                if (ws != null)
                {
                    muscleGroups = ws.ExerciseLogs.Select(l => l.MuscleGroup).Distinct().ToList();
                    workoutName = string.Join(" & ", muscleGroups.Select(m => char.ToUpper(m[0]) + m.Substring(1)).Take(3));
                    if (string.IsNullOrEmpty(workoutName)) workoutName = "Workout";
                    duration = (int)Math.Round(ws.DurationSeconds / 60.0);
                    exerciseCount = ws.ExerciseLogs.Count;
                    intensity = ws.IntensityScore <= 25 ? "Light" : ws.IntensityScore <= 50 ? "Moderate" : ws.IntensityScore <= 75 ? "High" : "Extreme";
                }

                var diff = DateTime.UtcNow - p.CreatedAt;
                var timeAgo = diff.TotalMinutes < 60 ? "Just now"
                    : diff.TotalHours < 24 ? $"{(int)diff.TotalHours}h ago"
                    : diff.TotalDays < 1.5 ? "Yesterday"
                    : diff.TotalDays < 7 ? $"{(int)diff.TotalDays}d ago"
                    : $"{(int)(diff.TotalDays / 7)}w ago";

                return (object)new
                {
                    PostId = p.Id,
                    UserName = p.User?.UserName ?? "Unknown",
                    TimeAgo = timeAgo,
                    Content = p.Content,
                    WorkoutName = workoutName,
                    Duration = duration.ToString(),
                    ExerciseCount = exerciseCount,
                    Intensity = intensity,
                    Likes = p.Likes,
                    Comments = p.Comments,
                    MuscleGroups = muscleGroups,
                    PhotoUrl = p.PhotoUrl ?? "",
                    ProgressSnapshot = p.ProgressSnapshot ?? "",
                    IsLiked = likedPostIds.Contains(p.Id)
                };
            }).ToList();

            ViewBag.Posts = posts;

            // Load user's past workouts for the "Log Workout" picker
            var userWorkouts = await _context.WorkoutSessions
                .Include(s => s.ExerciseLogs)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CompletedAt)
                .Take(20)
                .ToListAsync();

            var workoutOptions = userWorkouts.Select(ws =>
            {
                var muscles = ws.ExerciseLogs.Select(l => l.MuscleGroup).Distinct().ToList();
                var name = string.Join(" & ", muscles.Select(m => char.ToUpper(m[0]) + m.Substring(1)).Take(3));
                if (string.IsNullOrEmpty(name)) name = "Workout";
                var date = ws.CompletedAt.ToString("MMM d");
                var dur = (int)Math.Round(ws.DurationSeconds / 60.0);
                return (object)new { Id = ws.Id, Name = name, Date = date, Duration = dur, ExerciseCount = ws.ExerciseLogs.Count };
            }).ToList();

            ViewBag.UserWorkouts = workoutOptions;

            // User stats for progress snapshot
            var stats = await _workoutStatsService.GetDashboardStatsAsync(userId);
            ViewBag.UserStats = stats;

            // Load challenges from DB
            var challenges = await _context.Challenges
                .Include(c => c.Participants)
                .OrderByDescending(c => c.ParticipantCount)
                .ToListAsync();

            var joinedIds = challenges
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Select(c => c.Id)
                .ToHashSet();

            ViewBag.Challenges = challenges.Select(c => (object)new
            {
                c.Id, c.Name, c.Icon, c.Color, c.DurationDays, c.Difficulty,
                ParticipantCount = c.ParticipantCount,
                IsJoined = joinedIds.Contains(c.Id)
            }).ToList();

            // Real active users (users with workouts in last 7 days)
            var recentActiveUsers = await _context.WorkoutSessions
                .Where(s => s.CompletedAt >= DateTime.UtcNow.AddDays(-7))
                .Include(s => s.User)
                .Select(s => s.User!.UserName)
                .Distinct()
                .Take(8)
                .ToListAsync();
            ViewBag.ActiveUsers = recentActiveUsers;

            // Real top contributors (by workout count)
            var topContributors = await _context.WorkoutSessions
                .Include(s => s.User)
                .GroupBy(s => s.UserId)
                .Select(g => new { UserId = g.Key, UserName = g.First().User!.UserName, Workouts = g.Count() })
                .OrderByDescending(x => x.Workouts)
                .Take(3)
                .ToListAsync();
            ViewBag.TopContributors = topContributors.Select((c, i) => (object)new { Name = c.UserName ?? "Unknown", Workouts = c.Workouts, Rank = i + 1 }).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = new CommunityPost
            {
                UserId = userId,
                Content = request.Content ?? "",
                WorkoutSessionId = request.WorkoutSessionId,
                PhotoUrl = request.PhotoUrl,
                ProgressSnapshot = request.ProgressSnapshot,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunityPosts.Add(post);
            await _context.SaveChangesAsync();

            return Json(new { success = true, postId = post.Id });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existing = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == request.PostId && l.UserId == userId);

            var post = await _context.CommunityPosts.FindAsync(request.PostId);
            if (post == null) return NotFound();

            bool liked;
            if (existing != null)
            {
                _context.PostLikes.Remove(existing);
                post.Likes = Math.Max(0, post.Likes - 1);
                liked = false;
            }
            else
            {
                _context.PostLikes.Add(new PostLike { PostId = request.PostId, UserId = userId });
                post.Likes++;
                liked = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, liked, likes = post.Likes });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.CommunityPosts.FindAsync(request.PostId);
            if (post == null) return NotFound();

            var comment = new PostComment
            {
                PostId = request.PostId,
                UserId = userId,
                Content = request.Content ?? "",
                CreatedAt = DateTime.UtcNow
            };

            _context.PostComments.Add(comment);
            post.Comments++;
            await _context.SaveChangesAsync();

            var userName = User.Identity?.Name ?? "User";
            return Json(new { success = true, commentId = comment.Id, userName, content = comment.Content, comments = post.Comments });
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.PostComments
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    userName = c.User != null ? c.User.UserName : "Unknown",
                    content = c.Content,
                    timeAgo = FormatTimeAgo(c.CreatedAt)
                })
                .ToListAsync();

            return Json(comments);
        }

        private static string FormatTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 60) return "Just now";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 1.5) return "Yesterday";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return $"{(int)(diff.TotalDays / 7)}w ago";
        }

        public async Task<IActionResult> ChallengeDetail(int id)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var currentUser = User.Identity?.Name ?? "User";

            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null) return RedirectToAction("Community");

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

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> JoinChallenge([FromBody] ChallengeIdRequest request)
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
        public async Task<IActionResult> LeaveChallenge([FromBody] ChallengeIdRequest request)
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

        [AllowAnonymous]
        [Route("Home/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("NotFound");
                default:
                    return View("ServerError");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}