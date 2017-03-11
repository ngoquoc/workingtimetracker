using System;
using System.Data.Entity;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Implementations.Database;
using System.Linq;

namespace WorkingTimeTracker.Implementations.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WorkingTimeTrackerDbContext dbContext;

        public UserRepository(WorkingTimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        Task IUserRepository.CreateUser(User user)
        {
            dbContext.Users.Add(user);
            return dbContext.SaveChangesAsync();
        }

        Task IUserRepository.DeleteUser(User user, bool cascadeDelete)
        {
            if (cascadeDelete)
            {
                user = dbContext.Users
                    .Include(u => u.TimeEntries)
                    .FirstOrDefault(u => u.Id == user.Id);
            }

            dbContext.Users.Remove(user);
            return dbContext.SaveChangesAsync();
        }

        Task<User> IUserRepository.GetUserById(string id)
        {
            return dbContext.Users
                .Include(u => u.TimeEntries)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        IQueryable<User> IUserRepository.GetUsers()
        {
            return dbContext.Users.AsNoTracking();
        }

        async Task<bool> IUserRepository.IsEmailUnique(string id, string email)
        {
            var isEmailInUsed = await dbContext.Users.AsNoTracking()
                .AnyAsync(u => u.Id != id 
                    && email.ToLower() == u.Email.ToLower());

            return !isEmailInUsed;
        }

        Task IUserRepository.UpdateUser(User user)
        {
            dbContext.Entry(user).State = EntityState.Modified;
            return dbContext.SaveChangesAsync();
        }
    }
}
