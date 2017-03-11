using System;

namespace WorkingTimeTracker.Core.Entities
{
    public class TimeEntryWithOwnerName
    {
        public TimeEntryWithOwnerName(TimeEntry timeEntry)
        {
            this.Id = timeEntry.Id;
            this.Date = timeEntry.Date;
            this.Note = timeEntry.Note;
            this.Duration = timeEntry.Duration;
            this.OwnerId = timeEntry.OwnerId;
            this.OwnerName = timeEntry.Owner?.Name;
        }

        public Guid Id { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Note { get; set; }

        public double Duration { get; set; }

        public string OwnerId { get; set; }

        public string OwnerName { get; set; }

        public bool IsUnderPreferredWorkingHourPerDay { get; set; }
    }
}
