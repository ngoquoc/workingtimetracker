using System;

namespace WorkingTimeTracker.Core.Commands
{
    public class UpsertTimeEntryCommand
    {
        public Guid Id { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Note { get; set; }

        public double Duration { get; set; }

        public string OwnerId { get; set; }
    }
}
