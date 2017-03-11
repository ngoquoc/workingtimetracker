namespace WorkingTimeTracker.Core.Queries
{
    public class GetTimeEntryQuery
    {
        public string QueryString { get; set; }

        public bool IncludeAllUsers { get; set; }

        public int? PageSize { get; set; }

        public string OrderBy { get; set; }
    }
}
