using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Authorization;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Queries;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Core.Validators;

namespace WorkingTimeTracker.Core.Services.Implementations
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly IValidationService validationService;
        private readonly IAuthorizationService authorizationService;
        private readonly IQueryParser queryParser;
        private readonly ITimeEntryRepository timeEntryRepository;
        private readonly ICurrentUserResolver currentUserResolver;

        public TimeEntryService(IValidationService validationService,
            IAuthorizationService authorizationService,
            IQueryParser queryParser,
            ITimeEntryRepository timeEntryRepository,
            ICurrentUserResolver currentUserResolver)
        {
            this.validationService = validationService;
            this.authorizationService = authorizationService;
            this.queryParser = queryParser;
            this.timeEntryRepository = timeEntryRepository;
            this.currentUserResolver = currentUserResolver;
        }

        async Task ITimeEntryService.DeleteTimeEntry(DeleteTimeEntryCommand command)
        {
            var currentPrincipal = await currentUserResolver.ResolveCurrentClaimsPrincipalAsync();
            await authorizationService.AuthorizeResourceType(currentPrincipal, Operation.Delete, typeof(TimeEntry));

            Check.NotNull(command, errorMessage: "Command can not be null.");
            await validationService.Validate(command);

            var existingTimeEntry = await timeEntryRepository.GetTimeEntryById(command.TimeEntryId);
            if (existingTimeEntry == null)
            {
                return;
            }

            await authorizationService.AuthorizeResource(currentPrincipal, Operation.Delete, existingTimeEntry);
            await timeEntryRepository.DeleteTimeEntry(existingTimeEntry);
        }

        async Task<TimeEntrySummaryReportItem[]> ITimeEntryService.GenerateSummaryReport(GenerateTimeEntrySummaryReportQuery query)
        {
            var operation = query.IncludeTimeEntriesOfAllUsers ? Operation.ReadAll : Operation.Read;
            var currentPrincipal = await currentUserResolver.ResolveCurrentClaimsPrincipalAsync();
            await authorizationService.AuthorizeResourceType(currentPrincipal, operation, typeof(TimeEntry));

            Check.NotNull(query, "Query can not be null.");
            await validationService.Validate(query);

            var timeEntries = timeEntryRepository.GetTimeEntries();
            
            if (!query.IncludeTimeEntriesOfAllUsers)
            {
                var currentUser = await currentUserResolver.ResolveAsync();
                timeEntries = timeEntries.Where(te => te.OwnerId == currentUser.Id);
            }

            if (!string.IsNullOrWhiteSpace(query.QueryString))
            {
                try
                {
                    var parseResult = await queryParser.ApplyQuery(timeEntries, query.QueryString);
                    timeEntries = parseResult.Results;
                }
                catch (Exception ex)
                {
                    throw new ValidationException("Invalid query string.", ex);
                }
            }
            timeEntries = timeEntries
                .OrderBy(te => te.OwnerId)
                .ThenBy(te => te.Date);

            var resultIndex = new Dictionary<string, List<TimeEntry>>();

            foreach (var entry in timeEntries)
            {
                var serializedIndex = SerializeIndex(entry.OwnerId, entry.Date);
                if (!resultIndex.ContainsKey(serializedIndex))
                {
                    resultIndex.Add(serializedIndex, new List<TimeEntry>());
                }

                resultIndex[serializedIndex].Add(entry);
            }

            return resultIndex.Values.Select(r =>
            {
                var firstTimeEntry = r.First();

                var reportItem = new TimeEntrySummaryReportItem()
                {
                    OwnerId = firstTimeEntry.OwnerId,
                    OwnerName = firstTimeEntry.Owner.Name,
                    Date = firstTimeEntry.Date.Date,
                    Notes = r.Select(te => te.Note).ToArray(),
                    TotalTime = r.Sum(te => te.Duration)
                };
                reportItem.IsUnderPreferredWorkingHoursPerDay = 
                    firstTimeEntry.Owner.PreferredWorkingHourPerDay > reportItem.TotalTime;

                return reportItem;
            })
            .ToArray();
        }

        private string SerializeIndex(string ownerId, DateTimeOffset date)
        {
            return $"{ownerId}|{date.Date.ToShortDateString()}";
        }

        async Task<PagedResult<TimeEntryWithOwnerName>> ITimeEntryService.GetTimeEntries(GetTimeEntryQuery query)
        {
            Check.NotNull(query, errorMessage: "Query must be specified.");

            var currentPrincipal = await currentUserResolver.ResolveCurrentClaimsPrincipalAsync();
            var operation = query.IncludeAllUsers ? Operation.ReadAll : Operation.Read;
            await authorizationService.AuthorizeResourceType(currentPrincipal, operation, typeof(TimeEntry));

            var timeEntries = timeEntryRepository.GetTimeEntries();

            var currentUser = await currentUserResolver.ResolveAsync();
            if (operation == Operation.Read)
            {
                timeEntries = timeEntries.Where(te => te.OwnerId == currentUser.Id);
            }

            if (string.IsNullOrWhiteSpace(query.OrderBy))
            {
                timeEntries = timeEntries.OrderBy(te => te.Date);
            }

            var pagedResult = new PagedResult<TimeEntryWithOwnerName>();

            if (!string.IsNullOrEmpty(query.QueryString))
            {
                try
                {
                    var queryParserResult = await queryParser.ApplyQuery(timeEntries, query.QueryString);
                    timeEntries = queryParserResult.Results;
                    pagedResult.TotalCount = queryParserResult.TotalCount;
                }
                catch (Exception e)
                {
                    throw new ValidationException("Invalid query string.", e);
                }
            }

            var pageSize = Constants.MaxPageSize;
            if (query.PageSize.HasValue && query.PageSize < Constants.MaxPageSize)
            {
                pageSize = query.PageSize.Value;
            }

            timeEntries = timeEntries.Take(pageSize);

            var preferredWorkingHourIndex = new Dictionary<string, double>();
            foreach (var te in timeEntries)
            {
                var key = te.Date.Date.ToShortDateString() + "|" + te.OwnerId;
                if (!preferredWorkingHourIndex.ContainsKey(key))
                {
                    preferredWorkingHourIndex.Add(key, 0);
                }

                preferredWorkingHourIndex[key] += te.Duration;
            }

            var timeEntriesWithOwnerName = new List<TimeEntryWithOwnerName>();
            foreach (var te in timeEntries)
            {
                var key = te.Date.Date.ToShortDateString() + "|" + te.OwnerId;
                timeEntriesWithOwnerName.Add(new TimeEntryWithOwnerName(te)
                {
                    IsUnderPreferredWorkingHourPerDay = preferredWorkingHourIndex[key] < te.Owner.PreferredWorkingHourPerDay
                });
            }

            pagedResult.Results = timeEntriesWithOwnerName.ToArray();
            return pagedResult;
        }

        async Task<TimeEntry> ITimeEntryService.UpsertTimeEntry(UpsertTimeEntryCommand command)
        {
            var principal = await currentUserResolver.ResolveCurrentClaimsPrincipalAsync();
            await authorizationService.AuthorizeResourceType(principal, Operation.Upsert, typeof(TimeEntry));

            Check.NotNull(command, errorMessage: "Command can not be null.");
            await validationService.Validate(command);

            var timeEntryEntity = await timeEntryRepository.GetTimeEntryById(command.Id);
            if (timeEntryEntity != null)
            {
                await authorizationService.AuthorizeResource(principal, Operation.Update, timeEntryEntity);
                timeEntryEntity.Duration = command.Duration;
                timeEntryEntity.Note = command.Note;
                timeEntryEntity.Date = command.Date;

                if (timeEntryEntity.OwnerId != command.OwnerId)
                {
                    // If owner ID is changed, check if current principal has permission to do so
                    timeEntryEntity.OwnerId = command.OwnerId;
                    await authorizationService.AuthorizeResource(principal, Operation.Update, timeEntryEntity);
                }

                await timeEntryRepository.UpdateTimeEntry(timeEntryEntity);
            }
            else
            {
                timeEntryEntity = new TimeEntry()
                {
                    Id = command.Id,
                    Date = command.Date,
                    Duration = command.Duration,
                    Note = command.Note,
                    OwnerId = command.OwnerId
                };
                await authorizationService.AuthorizeResource(principal, Operation.Create, timeEntryEntity);

                await timeEntryRepository.CreateTimeEntry(timeEntryEntity);
            }

            return timeEntryEntity;
        }
    }
}
