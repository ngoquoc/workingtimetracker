using System;

namespace WorkingTimeTracker.Core.Entities
{
    public class TimeEntry
    {
        public Guid Id { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Note { get; set; }

        public double Duration { get; set; }

        public string OwnerId { get; set; }
        public User Owner { get; set; }
    }
}
