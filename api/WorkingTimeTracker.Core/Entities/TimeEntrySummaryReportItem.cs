using System;

namespace WorkingTimeTracker.Core.Entities
{
    public class TimeEntrySummaryReportItem
    {
        public DateTimeOffset Date { get; set; }

        public double TotalTime { get; set; }

        public string[] Notes { get; set; }

        public string OwnerId { get; set; }

        public string OwnerName { get; set; }

        public bool IsUnderPreferredWorkingHoursPerDay { get; set; }
    }
}
