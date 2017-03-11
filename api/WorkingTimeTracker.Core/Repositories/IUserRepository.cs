using System.Linq;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserById(string id);

        Task UpdateUser(User user);

        Task CreateUser(User user);

        Task<bool> IsEmailUnique(string id, string email);

        Task DeleteUser(User user, bool cascadeDelete);

        IQueryable<User> GetUsers();
    }
}
