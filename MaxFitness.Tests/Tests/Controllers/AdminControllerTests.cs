using Final_project___MaxFitness.Controllers;
using Final_project___MaxFitness.Data;
using Final_project___MaxFitness.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using Moq;

using System.Linq.Expressions;

namespace MaxFitness.Tests.Tests.Controllers
{
    public class AdminControllerTests
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

        private Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object, null, null, null, null);
        }

        private AdminController BuildController(
            AppDbContext db,
            Mock<UserManager<IdentityUser>> userMgr,
            Mock<RoleManager<IdentityRole>> roleMgr)
        {
            var controller = new AdminController(db, userMgr.Object, roleMgr.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return controller;
        }

        // ── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_ReturnsViewResultWithStats()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            var roleMgr = GetMockRoleManager();

            // Provide an async-capable IQueryable so EF CountAsync works
            var users = new List<IdentityUser>();
            var asyncUsers = new TestAsyncEnumerable<IdentityUser>(users.AsQueryable());
            userMgr.Setup(m => m.Users).Returns(asyncUsers);

            var controller = BuildController(db, userMgr, roleMgr);

            var result = await controller.Index();

            Assert.IsType<ViewResult>(result);
            Assert.Equal(0, controller.ViewBag.TotalWorkouts);
            Assert.Equal(0, controller.ViewBag.TotalPosts);
        }

        // ── DeletePost ────────────────────────────────────────────────────────

        [Fact]
        public async Task DeletePost_RemovesPostAndRelatedCommentsAndLikes()
        {
            using var db = GetDbContext();
            var userId = "admin-user";

            var post = new CommunityPost { UserId = userId, Content = "Test post", CreatedAt = DateTime.UtcNow };
            db.CommunityPosts.Add(post);
            await db.SaveChangesAsync();

            db.PostComments.Add(new PostComment { PostId = post.Id, UserId = userId, Content = "comment", CreatedAt = DateTime.UtcNow });
            db.PostLikes.Add(new PostLike { PostId = post.Id, UserId = userId });
            await db.SaveChangesAsync();

            var userMgr = GetMockUserManager();
            var roleMgr = GetMockRoleManager();
            var controller = BuildController(db, userMgr, roleMgr);

            var result = await controller.DeletePost(new IntIdRequest { Id = post.Id });

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(0, await db.CommunityPosts.CountAsync());
            Assert.Equal(0, await db.PostComments.CountAsync());
            Assert.Equal(0, await db.PostLikes.CountAsync());
        }

        // ── DeleteChallenge ───────────────────────────────────────────────────

        [Fact]
        public async Task DeleteChallenge_RemovesChallengeAndParticipants()
        {
            using var db = GetDbContext();

            var challenge = new Challenge
            {
                Name = "Test Challenge",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30)
            };
            db.Challenges.Add(challenge);
            await db.SaveChangesAsync();

            db.ChallengeParticipants.Add(new ChallengeParticipant
            {
                ChallengeId = challenge.Id,
                UserId = "some-user",
                JoinedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var userMgr = GetMockUserManager();
            var roleMgr = GetMockRoleManager();
            var controller = BuildController(db, userMgr, roleMgr);

            var result = await controller.DeleteChallenge(new IntIdRequest { Id = challenge.Id });

            Assert.IsType<JsonResult>(result);
            Assert.Equal(0, await db.Challenges.CountAsync());
            Assert.Equal(0, await db.ChallengeParticipants.CountAsync());
        }

        // ── CreateChallenge ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateChallenge_SavesNewChallengeToDb()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            var roleMgr = GetMockRoleManager();
            var controller = BuildController(db, userMgr, roleMgr);

            var request = new CreateChallengeRequest
            {
                Name = "Iron Week",
                Description = "Train every day for a week",
                Icon = "fa-fire",
                Color = "#ff0000",
                DurationDays = 7,
                Rules = "Log each session",
                Difficulty = "Advanced"
            };

            var result = await controller.CreateChallenge(request);

            Assert.IsType<JsonResult>(result);
            Assert.Equal(1, await db.Challenges.CountAsync());
            var saved = await db.Challenges.FirstAsync();
            Assert.Equal("Iron Week", saved.Name);
            Assert.Equal(7, saved.DurationDays);
        }

        // ── ToggleAdmin ───────────────────────────────────────────────────────

        [Fact]
        public async Task ToggleAdmin_NonAdminUser_AddsAdminRole()
        {
            using var db = GetDbContext();
            var userMgr = GetMockUserManager();
            var roleMgr = GetMockRoleManager();

            var user = new IdentityUser { Id = "user-toggle", UserName = "regular" };
            userMgr.Setup(m => m.FindByIdAsync("user-toggle")).ReturnsAsync(user);
            userMgr.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            userMgr.Setup(m => m.AddToRoleAsync(user, "Admin"))
                   .ReturnsAsync(IdentityResult.Success);

            var controller = BuildController(db, userMgr, roleMgr);

            var result = await controller.ToggleAdmin(new IdRequest { Id = "user-toggle" });

            Assert.IsType<JsonResult>(result);
            userMgr.Verify(m => m.AddToRoleAsync(user, "Admin"), Times.Once);
        }
    }

    // ── Async queryable helper ────────────────────────────────────────────────
    // Allows mocking UserManager.Users with EF async-compatible IQueryable.

    internal class TestAsyncEnumerable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _inner;

        public TestAsyncEnumerable(IQueryable<T> inner)
        {
            _inner = inner;
            Provider = new TestAsyncQueryProvider<T>(inner.Provider);
        }

        public Type ElementType => _inner.ElementType;
        public Expression Expression => _inner.Expression;
        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _inner.GetEnumerator();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(_inner.GetEnumerator());
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(_inner.CreateQuery<TEntity>(expression));
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(_inner.CreateQuery<TElement>(expression));
        public object? Execute(Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider)
                .GetMethods()
                .Single(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
                .MakeGenericMethod(resultType);

            var result = executeMethod.Invoke(_inner, new object[] { expression })!;
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { result })!;
        }
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

        public T Current => _inner.Current;
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    }
}
