using Final_project___MaxFitness.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Final_project___MaxFitness.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutSession> WorkoutSessions { get; set; }
        public DbSet<WorkoutExerciseLog> WorkoutExerciseLogs { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<ChallengeParticipant> ChallengeParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This is required for Identity to setup its internal tables (Users, Roles, etc.)
            base.OnModelCreating(modelBuilder);

            // Configure relationship: A WorkoutSession belongs to one User
            modelBuilder.Entity<WorkoutSession>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Data for Exercises
            modelBuilder.Entity<Exercise>().HasData(
                new Exercise { Id = 1, Name = "Bench Press", MuscleGroup = "chest", Type = "Compound" },
                new Exercise { Id = 2, Name = "Incline Dumbbell Press", MuscleGroup = "chest", Type = "Compound" },
                new Exercise { Id = 3, Name = "Cable Flyes", MuscleGroup = "chest", Type = "Isolation" },
                new Exercise { Id = 4, Name = "Push-Ups", MuscleGroup = "chest", Type = "Bodyweight" },
                new Exercise { Id = 5, Name = "Dumbbell Pullover", MuscleGroup = "chest", Type = "Isolation" },

                new Exercise { Id = 6, Name = "Deadlift", MuscleGroup = "back", Type = "Compound" },
                new Exercise { Id = 7, Name = "Lat Pulldown", MuscleGroup = "back", Type = "Compound" },
                new Exercise { Id = 8, Name = "Barbell Row", MuscleGroup = "back", Type = "Compound" },
                new Exercise { Id = 9, Name = "Seated Cable Row", MuscleGroup = "back", Type = "Isolation" },
                new Exercise { Id = 10, Name = "Face Pulls", MuscleGroup = "back", Type = "Isolation" },

                new Exercise { Id = 11, Name = "Overhead Press", MuscleGroup = "shoulders", Type = "Compound" },
                new Exercise { Id = 12, Name = "Lateral Raises", MuscleGroup = "shoulders", Type = "Isolation" },
                new Exercise { Id = 13, Name = "Front Raises", MuscleGroup = "shoulders", Type = "Isolation" },
                new Exercise { Id = 14, Name = "Reverse Flyes", MuscleGroup = "shoulders", Type = "Isolation" },
                new Exercise { Id = 15, Name = "Arnold Press", MuscleGroup = "shoulders", Type = "Compound" },

                new Exercise { Id = 16, Name = "Barbell Curl", MuscleGroup = "biceps", Type = "Isolation" },
                new Exercise { Id = 17, Name = "Hammer Curls", MuscleGroup = "biceps", Type = "Isolation" },
                new Exercise { Id = 18, Name = "Incline Dumbbell Curls", MuscleGroup = "biceps", Type = "Isolation" },
                new Exercise { Id = 19, Name = "Preacher Curls", MuscleGroup = "biceps", Type = "Isolation" },
                new Exercise { Id = 20, Name = "Cable Curls", MuscleGroup = "biceps", Type = "Isolation" },

                new Exercise { Id = 21, Name = "Tricep Dips", MuscleGroup = "triceps", Type = "Compound" },
                new Exercise { Id = 22, Name = "Skull Crushers", MuscleGroup = "triceps", Type = "Isolation" },
                new Exercise { Id = 23, Name = "Tricep Pushdown", MuscleGroup = "triceps", Type = "Isolation" },
                new Exercise { Id = 24, Name = "Overhead Tricep Extension", MuscleGroup = "triceps", Type = "Isolation" },
                new Exercise { Id = 25, Name = "Close-Grip Bench Press", MuscleGroup = "triceps", Type = "Compound" },

                new Exercise { Id = 26, Name = "Barbell Squat", MuscleGroup = "legs", Type = "Compound" },
                new Exercise { Id = 27, Name = "Romanian Deadlift", MuscleGroup = "legs", Type = "Compound" },
                new Exercise { Id = 28, Name = "Leg Press", MuscleGroup = "legs", Type = "Compound" },
                new Exercise { Id = 29, Name = "Leg Curl", MuscleGroup = "legs", Type = "Isolation" },
                new Exercise { Id = 30, Name = "Calf Raises", MuscleGroup = "legs", Type = "Isolation" },
                new Exercise { Id = 31, Name = "Bulgarian Split Squat", MuscleGroup = "legs", Type = "Compound" },

                new Exercise { Id = 32, Name = "Crunches", MuscleGroup = "abs", Type = "Bodyweight" },
                new Exercise { Id = 33, Name = "Hanging Leg Raises", MuscleGroup = "abs", Type = "Bodyweight" },
                new Exercise { Id = 34, Name = "Plank", MuscleGroup = "abs", Type = "Bodyweight" },
                new Exercise { Id = 35, Name = "Cable Woodchops", MuscleGroup = "abs", Type = "Isolation" },
                new Exercise { Id = 36, Name = "Ab Wheel Rollout", MuscleGroup = "abs", Type = "Bodyweight" },

                new Exercise { Id = 37, Name = "Wrist Curls", MuscleGroup = "forearms", Type = "Isolation" },
                new Exercise { Id = 38, Name = "Reverse Wrist Curls", MuscleGroup = "forearms", Type = "Isolation" },
                new Exercise { Id = 39, Name = "Farmer's Walk", MuscleGroup = "forearms", Type = "Compound" },
                new Exercise { Id = 40, Name = "Plate Pinch", MuscleGroup = "forearms", Type = "Isolation" }
            );

            // Seed Challenges
            modelBuilder.Entity<Challenge>().HasData(
                new Challenge { Id = 1, Name = "30-Day Squat Challenge", Description = "Complete a squat variation every day for 30 days. Start with bodyweight and progressively add weight. Build insane leg strength and discipline.", Icon = "fa-person-running", Color = "#e63946", DurationDays = 30, StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2026, 3, 31), Rules = "Do at least one set of squats every day;Minimum 10 reps per set;Rest days allowed but must make up the next day;Log every session in MaxFitness", Difficulty = "Intermediate" },
                new Challenge { Id = 2, Name = "5K Steps Daily", Description = "Hit 5,000 steps minimum every single day. A simple but powerful habit that keeps you moving even on rest days. Perfect for active recovery.", Icon = "fa-shoe-prints", Color = "#ff6b35", DurationDays = 30, StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2026, 3, 31), Rules = "Walk at least 5,000 steps every day;Track with any step counter or fitness watch;Post your daily count in the community;Miss a day and you're out", Difficulty = "Beginner" },
                new Challenge { Id = 3, Name = "No Rest Day Week", Description = "Train every single day for 7 days straight. Mix up muscle groups, intensity, and workout types. Test your mental and physical limits.", Icon = "fa-fire", Color = "#ffd166", DurationDays = 7, StartDate = new DateTime(2026, 3, 24), EndDate = new DateTime(2026, 3, 31), Rules = "Work out every day for 7 consecutive days;Each session must be at least 30 minutes;Vary muscle groups to prevent overtraining;Log all sessions in MaxFitness", Difficulty = "Advanced" },
                new Challenge { Id = 4, Name = "Push-Up Master", Description = "Work your way up to 100 push-ups in a single session. Start wherever you are and build up over 4 weeks. Upper body gains guaranteed.", Icon = "fa-hand-fist", Color = "#22c55e", DurationDays = 28, StartDate = new DateTime(2026, 3, 3), EndDate = new DateTime(2026, 3, 31), Rules = "Do push-ups at least 5 days per week;Track your max set each session;Goal is 100 push-ups in one session by day 28;Any push-up variation counts", Difficulty = "Intermediate" },
                new Challenge { Id = 5, Name = "Iron Streak", Description = "Maintain a 14-day consecutive workout streak. No excuses, no skipping. Build the habit that separates the dedicated from the rest.", Icon = "fa-link", Color = "#8b5cf6", DurationDays = 14, StartDate = new DateTime(2026, 3, 17), EndDate = new DateTime(2026, 3, 31), Rules = "Work out every day for 14 days;Minimum 20 minutes per session;Any workout type counts;Log in MaxFitness to track your streak", Difficulty = "Advanced" }
            );
        }
    }
}