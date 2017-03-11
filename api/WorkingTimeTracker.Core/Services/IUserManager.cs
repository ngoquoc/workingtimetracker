using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Core.Services
{
    public interface IUserManager
    {
        Task UpdateUserRoles(string userId, string[] roles);

        Task CreateAsync(User user);

        Task RemoveUser(string userId);

        Task<string[]> GetRoles(string userId);

        Task<Dictionary<string, int>> CountUsersInRoles();
    }
}
