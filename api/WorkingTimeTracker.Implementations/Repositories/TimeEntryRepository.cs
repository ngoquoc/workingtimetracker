using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Implementations.Database;

namespace WorkingTimeTracker.Implementations.Repositories
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly WorkingTimeTrackerDbContext dbContext;

        public TimeEntryRepository(WorkingTimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        async Task<TimeEntry> ITimeEntryRepository.CreateTimeEntry(TimeEntry timeEntryEntity)
        {
            dbContext.TimeEntries.Add(timeEntryEntity);
            await dbContext.SaveChangesAsync();

            return timeEntryEntity;
        }

        Task ITimeEntryRepository.DeleteTimeEntry(TimeEntry timeEntry)
        {
            dbContext.TimeEntries.Remove(timeEntry);
            return dbContext.SaveChangesAsync();
        }

        IQueryable<TimeEntry> ITimeEntryRepository.GetTimeEntries()
        {
            return dbContext.TimeEntries
                .Include(te => te.Owner);
        }

        Task<TimeEntry> ITimeEntryRepository.GetTimeEntryById(Guid timeEntryId)
        {
            return dbContext.TimeEntries
                .Include(te => te.Owner)
                .FirstOrDefaultAsync(te => te.Id == timeEntryId);
        }

        async Task<TimeEntry> ITimeEntryRepository.UpdateTimeEntry(TimeEntry timeEntryEntity)
        {
            dbContext.Entry(timeEntryEntity).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return timeEntryEntity;
        }
    }
}
