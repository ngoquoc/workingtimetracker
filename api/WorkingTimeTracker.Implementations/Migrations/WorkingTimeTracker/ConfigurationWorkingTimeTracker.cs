using System.Data.Entity.Migrations;
using WorkingTimeTracker.Implementations.Database;

namespace WorkingTimeTracker.Implementations.Migrations.WorkingTimeTracker
{
    internal sealed class ConfigurationWorkingTimeTracker : DbMigrationsConfiguration<WorkingTimeTrackerDbContext>
    {
        public ConfigurationWorkingTimeTracker()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\WorkingTimeTracker";
        }

        protected override void Seed(WorkingTimeTrackerDbContext context)
        {
        }
    }
}
