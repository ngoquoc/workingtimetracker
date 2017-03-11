using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Repositories;

namespace WorkingTimeTracker.Core.Validators
{
    public class UpsertTimeEntryValidator : Validator<UpsertTimeEntryCommand>
    {
        private readonly IUserRepository userRepository;

        public UpsertTimeEntryValidator(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public override async Task Validate(UpsertTimeEntryCommand command)
        {
            Check.NotNull(command, errorMessage: "Command can not be null.");
            Check.NotEqual(command.Id, Guid.Empty, errorMessage: "Invalid time entry ID.");
            Check.NotEmpty(command.Note, errorMessage: "Note can not be empty.");
            Check.Max(command.Note.Length, 300, errorMessage: "Note must be shorter than 300 characters.");

            Check.Max(command.Duration, 24, errorMessage: "Duration can not be greater than 24 hours.");
            Check.GreaterThan(command.Duration, 0, errorMessage: "Duration must be greater than 0.");

            Check.NotEmpty(command.OwnerId, errorMessage: "Invalid owner ID.");
            Check.NotEqual(command.OwnerId, Guid.Empty.ToString(), errorMessage: "Invalid OwnerId.");
            var user = await userRepository.GetUserById(command.OwnerId);
            Check.NotNull(user, errorMessage: "Invalid owner ID.");
        }
    }
}
