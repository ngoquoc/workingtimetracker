using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;

namespace WorkingTimeTracker.Core.Validators
{
    public class RegisterCommandValidator : Validator<RegisterCommand>
    {
        public override Task Validate(RegisterCommand command)
        {
            Check.NotEmpty(command.Email, "Email");
            Check.RegEx(command.Email, "^.+@.+$",
                errorMessage: "Wrong email format.");

            Check.NotEmpty(command.Name, "Name");
            Check.Max(command.Name.Length, 100,
                errorMessage: "Name must be shorter than 100 characters.");

            Check.NotEmpty(command.Password, "Password");
            Check.Equal(command.PasswordConfirm, command.Password, 
                errorMessage: "Password confirmation does not match password.");
            Check.Min(command.Password.Length, 6,
                errorMessage: "Password must be at least 6 characters.");
            Check.PasswordStrength(command.Password);

            return Task.FromResult(0);
        }
    }
}
