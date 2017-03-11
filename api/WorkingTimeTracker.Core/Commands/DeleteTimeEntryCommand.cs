using System;

namespace WorkingTimeTracker.Core.Commands
{
    public class DeleteTimeEntryCommand
    {
        public Guid TimeEntryId { get; set; }
    }
}
