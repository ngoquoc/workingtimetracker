using System.Linq;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Core.Services
{
    public interface IQueryParser
    {
        Task<QueryParserResult<T>> ApplyQuery<T>(IQueryable<T> collection, string query);
    }
}
