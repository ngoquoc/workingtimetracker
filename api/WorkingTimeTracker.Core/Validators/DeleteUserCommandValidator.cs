using System.Linq;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Core.Services;

namespace WorkingTimeTracker.Core.Validators
{
    public class DeleteUserCommandValidator : Validator<DeleteUserCommand>
    {
        private readonly IUserManager userManager;
        private readonly IUserRepository userRepository;
        private readonly ICurrentUserResolver currentUserRolver;

        public DeleteUserCommandValidator(IUserManager userManager,
            IUserRepository userRepository,
            ICurrentUserResolver currentUserRolver)
        {
            this.userManager = userManager;
            this.userRepository = userRepository;
            this.currentUserRolver = currentUserRolver;
        }

        public override async Task Validate(DeleteUserCommand command)
        {
            Check.NotEmpty(command.UserId);

            var user = await userRepository.GetUserById(command.UserId);

            if (user == null)
            {
                return;
            }

            var currentUser = await currentUserRolver.ResolveAsync();
            Check.NotEqual(currentUser.Id, command.UserId, errorMessage: "Can not delete yourself.");

            var hasTimeEntries = user.TimeEntries != null && user.TimeEntries.Any();
            var hasTEButNotForceDelete = !command.ForceDelete && hasTimeEntries;
            Check.Equal(hasTEButNotForceDelete, false,
                errorMessage: "There are time entries associated with this user, consider force delete.");

            var userRoles = await userManager.GetRoles(command.UserId);
            if (!userRoles.Contains(Constants.ROLE_AMIN) && !userRoles.Contains(Constants.ROLE_USER_MANGER))
            {
                return;
            }

            var userCountByRole = await userManager.CountUsersInRoles();
            if (userCountByRole[Constants.ROLE_AMIN] + userCountByRole[Constants.ROLE_USER_MANGER] <= 1)
            {
                throw new ValidationException("This user is the only admin/user manager in system.");
            }
        }
    }
}
