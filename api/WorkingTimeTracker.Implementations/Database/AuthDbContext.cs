using Microsoft.AspNet.Identity.EntityFramework;

namespace WorkingTimeTracker.Implementations.Database
{
    public class AuthDbContext : IdentityDbContext
    {
        public AuthDbContext() : base("Auth")
        {
        }

        public AuthDbContext(string nameOrConnectionString) 
            : base(nameOrConnectionString)
        {
        }
    }
}
