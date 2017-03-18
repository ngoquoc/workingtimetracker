using System.Data.Entity;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Implementations.Database
{
    public class WorkingTimeTrackerDbContext : DbContext
    {
        public WorkingTimeTrackerDbContext() 
            : base("WorkingTimeTracker")
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<TimeEntry> TimeEntries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.TimeEntries)
                .WithRequired(te => te.Owner)
                .HasForeignKey(te => te.OwnerId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
