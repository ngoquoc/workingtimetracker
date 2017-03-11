using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;

namespace WorkingTimeTracker.Core.Validators
{
    public class ChangePasswordCommandValidator : Validator<ChangePasswordCommand>
    {
        public override Task Validate(ChangePasswordCommand command)
        {
            Check.NotEmpty(command.OldPassword, "Current password");
            Check.NotEmpty(command.NewPassword, "New password");
            Check.NotEqual(command.OldPassword, command.NewPassword, 
                errorMessage: "New password must be different from old password.");
            Check.Equal(command.NewPasswordConfirm, command.NewPassword, "New password confirm");
            Check.PasswordStrength(command.NewPassword);

            return Task.FromResult(0);
        }
    }
}
