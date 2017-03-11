using System.Linq;

namespace WorkingTimeTracker.Core.Entities
{
    public class QueryParserResult<T>
    {
        public int TotalCount { get; set; }

        public IQueryable<T> Results { get; set; }
    }
}
