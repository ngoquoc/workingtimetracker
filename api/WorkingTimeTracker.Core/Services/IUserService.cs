using System;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Queries;

namespace WorkingTimeTracker.Core.Services
{
    public interface IUserService
    {
        Task<PagedResult<UserWithRoles>> GetUsersWithRoles(GetUsersQuery query);

        Task<User> UpsertUser(UpsertUserCommand command);

        Task DeleteUser(DeleteUserCommand command);

        Task<CurrentUserData> GetCurrentUserWithRoles();

        Task UpdateCurrentUserSettings(UpdateCurrentUserSettingsCommand command);
    }

    public class DeleteUserException : Exception
    {
        public DeleteUserException(string message)
            : base(message) { }

        public DeleteUserException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
