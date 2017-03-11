using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Core.Repositories
{
    public interface ITimeEntryRepository
    {
        IQueryable<TimeEntry> GetTimeEntries();

        Task<TimeEntry> GetTimeEntryById(Guid timeEntryId);

        Task<TimeEntry> UpdateTimeEntry(TimeEntry timeEntryEntity);

        Task<TimeEntry> CreateTimeEntry(TimeEntry timeEntryEntity);

        Task DeleteTimeEntry(TimeEntry timeEntryEntity);
    }
}
