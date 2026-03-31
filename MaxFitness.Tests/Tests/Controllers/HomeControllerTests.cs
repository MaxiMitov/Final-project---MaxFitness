using Final_project___MaxFitness.Controllers;
using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;
using Final_project___MaxFitness.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace MaxFitness.Tests.Tests.Controllers
{
    public class HomeControllerTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private AppDbContext GetDbContext(string dbName = null)
        {
            dbName ??= Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        private Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private HomeController BuildController(
            Mock<UserManager<IdentityUser>> userManagerMock,
            Mock<IWorkoutStatsService> statsSvcMock,
            Mock<IMuscleService> muscleSvcMock,
            AppDbContext db)
        {
            var logger = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(
                logger.Object,
                muscleSvcMock.Object,
                statsSvcMock.Object,
                userManagerMock.Object,
                db);

            // Provide a default HttpContext so User is never null
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return controller;
        }

        // ── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_NullUserId_RedirectsToLogin()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            userMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                   .Returns((string?)null);

            var statsSvc  = new Mock<IWorkoutStatsService>();
            var muscleSvc = new Mock<IMuscleService>();

            var controller = BuildController(userMgr, statsSvc, muscleSvc, db);

            var result = await controller.Index();

            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Account/Login", redirect.PageName);
        }

        [Fact]
        public async Task Index_AuthenticatedUser_ReturnsViewWithViewBagData()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            userMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                   .Returns("user-123");

            var stats = new DashboardStats { WeeklyWorkouts = 3 };
            var statsSvc = new Mock<IWorkoutStatsService>();
            statsSvc.Setup(s => s.GetDashboardStatsAsync("user-123")).ReturnsAsync(stats);
            statsSvc.Setup(s => s.GetMuscleStatusesAsync("user-123")).ReturnsAsync(new List<MuscleStatus>());
            statsSvc.Setup(s => s.GetRecentWorkoutsAsync("user-123", 5)).ReturnsAsync(new List<RecentWorkout>());

            var muscleSvc = new Mock<IMuscleService>();
            var controller = BuildController(userMgr, statsSvc, muscleSvc, db);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Stats);
            Assert.NotNull(controller.ViewBag.MuscleStatuses);
            Assert.NotNull(controller.ViewBag.RecentWorkouts);
        }

        // ── Settings ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Settings_AuthenticatedUser_ReturnsViewWithUserInfo()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            var user = new IdentityUser { Id = "u1", UserName = "fituser", Email = "fit@test.com" };
            userMgr.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                   .ReturnsAsync(user);

            var profileStats = new UserProfileStats { MostTrainedMuscle = "Chest", MuscleTrainingCounts = new List<MuscleTrainingStat>() };
            var statsSvc = new Mock<IWorkoutStatsService>();
            statsSvc.Setup(s => s.GetUserProfileStatsAsync("u1")).ReturnsAsync(profileStats);

            var muscleSvc = new Mock<IMuscleService>();
            var controller = BuildController(userMgr, statsSvc, muscleSvc, db);

            var result = await controller.Settings();

            Assert.IsType<ViewResult>(result);
            Assert.Equal("fituser", controller.ViewBag.UserName);
            Assert.Equal("fit@test.com", controller.ViewBag.UserEmail);
        }

        // ── Leaderboard ───────────────────────────────────────────────────────

        [Fact]
        public async Task Leaderboard_ReturnsViewWithLeadersList()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            userMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                   .Returns("u1");

            var statsSvc  = new Mock<IWorkoutStatsService>();
            var muscleSvc = new Mock<IMuscleService>();
            var controller = BuildController(userMgr, statsSvc, muscleSvc, db);

            var result = await controller.Leaderboard();

            Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Leaders);
        }

        // ── Community ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Community_ReturnsViewWithPosts()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            userMgr.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                   .Returns("u1");

            var stats = new DashboardStats();
            var statsSvc = new Mock<IWorkoutStatsService>();
            statsSvc.Setup(s => s.GetDashboardStatsAsync(It.IsAny<string>())).ReturnsAsync(stats);

            var muscleSvc = new Mock<IMuscleService>();
            var controller = BuildController(userMgr, statsSvc, muscleSvc, db);

            var result = await controller.Community();

            Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Posts);
        }

        // ── Privacy ───────────────────────────────────────────────────────────

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            using var db = GetDbContext();
            var userMgr  = GetMockUserManager();
            var statsSvc  = new Mock<IWorkoutStatsService>();
            var muscleSvc = new Mock<IMuscleService>();
            var controller = BuildController(userMgr, statsSvc, muscleSvc, db);

            var result = controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }

        // ── HttpStatusCodeHandler ──────────────────────────────────────────────

        [Fact]
        public void HttpStatusCodeHandler_404_ReturnsNotFoundView()
        {
            using var db = GetDbContext();
            var userMgr  = GetMockUserManager();
            var statsSvc  = new Mock<IWorkoutStatsService>();
            var muscleSvc = new Mock<IMuscleService>();
            var controller = BuildController(userMgr, statsSvc, muscleSvc, db);

            var result = controller.HttpStatusCodeHandler(404);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("NotFound", view.ViewName);
        }

        [Fact]
        public void HttpStatusCodeHandler_500_ReturnsServerErrorView()
        {
            using var db = GetDbContext();
            var userMgr  = GetMockUserManager();
            var statsSvc  = new Mock<IWorkoutStatsService>();
            var muscleSvc = new Mock<IMuscleService>();
            var controller = BuildController(userMgr, statsSvc, muscleSvc, db);

            var result = controller.HttpStatusCodeHandler(500);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("ServerError", view.ViewName);
        }
    }
}
