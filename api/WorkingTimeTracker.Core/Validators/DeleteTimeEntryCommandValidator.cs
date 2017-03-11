using System;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;

namespace WorkingTimeTracker.Core.Validators
{
    public class DeleteTimeEntryCommandValidator : Validator<DeleteTimeEntryCommand>
    {
        public override Task Validate(DeleteTimeEntryCommand command)
        {
            Check.NotEqual(Guid.Empty, command.TimeEntryId, errorMessage: "Invalid time entry ID.");

            return Task.FromResult(0);
        }
    }
}
