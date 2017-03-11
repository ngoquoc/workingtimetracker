using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToQuerystring;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Services;

namespace WorkingTimeTracker.Implementations.Services
{
    public class QueryParser : IQueryParser
    {
        Task<QueryParserResult<T>> IQueryParser.ApplyQuery<T>(IQueryable<T> collection, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                query = "$inlinecount=allpages";
            }
            else
            {
                query += "&$inlinecount=allpages";
            }

            var result = collection.LinqToQuerystring<T, Dictionary<string, object>>(query);
            
            var queryParserResult = new QueryParserResult<T>();
            if (result.ContainsKey("Count"))
            {
                queryParserResult.TotalCount = (int)result["Count"];
            }
            if (result.ContainsKey("Results"))
            {
                queryParserResult.Results = result["Results"] as IQueryable<T>;
            }

            return Task.FromResult(queryParserResult);
        }
    }
}
