using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Repositories;

namespace WorkingTimeTracker.Core.Validators
{
    public class UpsertUserCommandValidator : Validator<UpsertUserCommand>
    {
        private readonly IUserRepository userRepository;

        public UpsertUserCommandValidator(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public override async Task Validate(UpsertUserCommand command)
        {
            Check.NotEmpty(command.Id, "Id");

            Check.NotEmpty(command.Name, "Name");
            Check.Max(command.Name.Length, 100, 
                errorMessage: "Name must be shorter than 100 characters.");

            Check.NotEmpty(command.Email, "Email");

            if (command.Roles != null)
            {
                Check.Min(command.Roles.Length, 1, 
                    errorMessage: "At least 1 role must be specified.");

                var supportedRoles = new[] { Constants.ROLE_AMIN, Constants.ROLE_USER_MANGER, Constants.ROLE_USER };
                foreach (var role in command.Roles)
                {
                    Check.In(role, supportedRoles, 
                        errorMessage: $"Unsupported role: {role}.");
                }
            }

            var isEmailUnique = await userRepository.IsEmailUnique(command.Id, command.Email);
            Check.True(isEmailUnique,
                errorMessage: "Email has been already used.");
        }
    }
}
