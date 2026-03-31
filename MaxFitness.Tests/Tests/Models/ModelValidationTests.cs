using System.ComponentModel.DataAnnotations;

using Final_project___MaxFitness.Models;

namespace MaxFitness.Tests.Tests.Models
{
    public class ModelValidationTests
    {
        private static IList<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        // ── CommunityPost ─────────────────────────────────────────────────────

        [Fact]
        public void CommunityPost_EmptyContent_FailsValidation()
        {
            var post = new CommunityPost { UserId = "u1", Content = "" };
            var results = Validate(post);
            Assert.Contains(results, r => r.MemberNames.Contains("Content"));
        }

        [Fact]
        public void CommunityPost_ContentOver5000Chars_FailsValidation()
        {
            var post = new CommunityPost { UserId = "u1", Content = new string('x', 5001) };
            var results = Validate(post);
            Assert.Contains(results, r => r.MemberNames.Contains("Content"));
        }

        [Fact]
        public void CommunityPost_ValidContent_PassesValidation()
        {
            var post = new CommunityPost { UserId = "u1", Content = "Great workout today!" };
            var results = Validate(post);
            Assert.DoesNotContain(results, r => r.MemberNames.Contains("Content"));
        }

        // ── Exercise ──────────────────────────────────────────────────────────

        [Fact]
        public void Exercise_EmptyName_FailsValidation()
        {
            var exercise = new Exercise { Name = "", MuscleGroup = "chest" };
            var results = Validate(exercise);
            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Exercise_EmptyMuscleGroup_FailsValidation()
        {
            var exercise = new Exercise { Name = "Bench Press", MuscleGroup = "" };
            var results = Validate(exercise);
            Assert.Contains(results, r => r.MemberNames.Contains("MuscleGroup"));
        }

        [Fact]
        public void Exercise_ValidExercise_PassesValidation()
        {
            var exercise = new Exercise { Name = "Bench Press", MuscleGroup = "chest", Type = "Compound" };
            var results = Validate(exercise);
            Assert.Empty(results);
        }

        // ── Challenge ─────────────────────────────────────────────────────────

        [Fact]
        public void Challenge_EmptyName_FailsValidation()
        {
            var challenge = new Challenge { Name = "" };
            var results = Validate(challenge);
            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Challenge_ValidChallenge_PassesValidation()
        {
            var challenge = new Challenge
            {
                Name = "Push-Up Master",
                Description = "100 push-ups",
                DurationDays = 28,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(28)
            };
            var results = Validate(challenge);
            Assert.DoesNotContain(results, r => r.MemberNames.Contains("Name"));
        }

        // ── WorkoutSession ────────────────────────────────────────────────────

        [Fact]
        public void WorkoutSession_EmptyUserId_FailsValidation()
        {
            var session = new WorkoutSession { UserId = "" };
            var results = Validate(session);
            Assert.Contains(results, r => r.MemberNames.Contains("UserId"));
        }
    }
}
