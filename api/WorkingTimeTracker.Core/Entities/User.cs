using System.Collections.Generic;

namespace WorkingTimeTracker.Core.Entities
{
    public class User
    {
        public User()
        {
            this.TimeEntries = new List<TimeEntry>();
        }

        public string Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public double PreferredWorkingHourPerDay { get; set; }

        public virtual ICollection<TimeEntry> TimeEntries { get; set; }
    }
}
