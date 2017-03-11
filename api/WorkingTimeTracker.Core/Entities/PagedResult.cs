namespace WorkingTimeTracker.Core.Entities
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }

        public T[] Results { get; set; }
    }
}
