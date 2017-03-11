using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Queries;

namespace WorkingTimeTracker.Core.Services
{
    public interface ITimeEntryService
    {
        Task<PagedResult<TimeEntryWithOwnerName>> GetTimeEntries(GetTimeEntryQuery query);

        Task<TimeEntry> UpsertTimeEntry(UpsertTimeEntryCommand command);

        Task DeleteTimeEntry(DeleteTimeEntryCommand command);

        Task<TimeEntrySummaryReportItem[]> GenerateSummaryReport(GenerateTimeEntrySummaryReportQuery query);
    }
}
