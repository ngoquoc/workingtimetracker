namespace WorkingTimeTracker.Core.Queries
{
    public class GetUsersQuery
    {
        public GetUsersQuery()
        {
            QueryString = string.Empty;
        }

        public bool ExcludeMe { get; set; }

        public string QueryString { get; set; }

        public int Top { get; set; }

        public string OrderBy { get; set; }
    }
}
