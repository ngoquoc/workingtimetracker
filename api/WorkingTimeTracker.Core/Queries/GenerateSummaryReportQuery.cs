using System;

namespace WorkingTimeTracker.Core.Queries
{
    public class GenerateTimeEntrySummaryReportQuery
    {
        public bool IncludeTimeEntriesOfAllUsers { get; set; }

        public string QueryString { get; set; }
    }
}
