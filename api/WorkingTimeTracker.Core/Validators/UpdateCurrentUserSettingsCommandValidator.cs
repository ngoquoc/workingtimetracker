using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;

namespace WorkingTimeTracker.Core.Validators
{
    public class UpdateCurrentUserSettingsCommandValidator : Validator<UpdateCurrentUserSettingsCommand>
    {
        public override Task Validate(UpdateCurrentUserSettingsCommand command)
        {
            Check.NotEmpty(command.Name);
            Check.GreaterThan(command.PreferredWorkingHourPerDay, 0,
                errorMessage: "Preferred working hours per day must be a positive number.");

            Check.Max(command.PreferredWorkingHourPerDay, 24,
                errorMessage: "Preferred working hours must be less than 24.");

            return Task.FromResult(0);
        }
    }
}
