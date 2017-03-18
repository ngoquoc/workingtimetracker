using System.Data.Entity.Migrations;
using Microsoft.AspNet.Identity.EntityFramework;
using WorkingTimeTracker.Core;
using WorkingTimeTracker.Implementations.Database;

namespace WorkingTimeTracker.Implementations.Migrations.Auth
{

    internal sealed class ConfigurationAuth : DbMigrationsConfiguration<AuthDbContext>
    {
        public ConfigurationAuth()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\Auth";
        }

        protected override void Seed(AuthDbContext context)
        {
            context.Roles.AddOrUpdate(
                r => r.Name,
                new IdentityRole(Constants.ROLE_AMIN),
                new IdentityRole(Constants.ROLE_USER_MANGER),
                new IdentityRole(Constants.ROLE_USER)
            );
        }
    }
}
