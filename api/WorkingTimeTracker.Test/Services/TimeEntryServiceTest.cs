using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WorkingTimeTracker.Core;
using WorkingTimeTracker.Core.Authorization;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Queries;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Core.Services;
using WorkingTimeTracker.Core.Services.Implementations;
using WorkingTimeTracker.Core.Validators;
using WorkingTimeTracker.Implementations.Services;
using Xunit;

namespace WorkingTimeTracker.Test.Services
{
    public class TimeEntryServiceTest
    {
        private ITimeEntryService InstantiateTimeEntryService(params Tuple<Type, object>[] dependencies)
        {
            var type = typeof(TimeEntryService);
            var constructor = type.GetConstructor(new[]
            {
                typeof(IValidationService),
                typeof(IAuthorizationService),
                typeof(IQueryParser),
                typeof(ITimeEntryRepository),
                typeof(ICurrentUserResolver)
            });

            var validationService = dependencies
                .FirstOrDefault(a => a.Item1 == typeof(IValidationService))
                ?.Item2;
            if (validationService == null)
            {
                validationService = ValidationService.DefaultInstance;
            }

            var authorizationService = dependencies
                .FirstOrDefault(a => a.Item1 == typeof(IAuthorizationService))
                ?.Item2;
            if (authorizationService == null)
            {
                authorizationService = new AuthorizationService();
            }

            var queryParser = dependencies
                .FirstOrDefault(a => a.Item1 == typeof(IQueryParser))
                ?.Item2;
            if (queryParser == null)
            {
                queryParser = new QueryParser();
            }

            var timeEntryRepository = dependencies
                .FirstOrDefault(a => a.Item1 == typeof(ITimeEntryRepository))
                ?.Item2;
            if (timeEntryRepository == null)
            {
                timeEntryRepository = new Mock<ITimeEntryRepository>().Object;
            }

            var currentUserResolver = dependencies
                .FirstOrDefault(a => a.Item1 == typeof(ICurrentUserResolver))
                ?.Item2;
            if (currentUserResolver == null)
            {
                currentUserResolver = new Mock<ICurrentUserResolver>().Object;
            }

            return constructor.Invoke(new [] {
                validationService,
                authorizationService,
                queryParser,
                timeEntryRepository,
                currentUserResolver
            }) as ITimeEntryService;
        }

        #region Get time entries

        [Fact(DisplayName = "When get time entries with page size greater than max page size, it takes max page size.")]
        public async Task GetTimeEntries_PageSizeGreaterThanMaxPageSize_TakeMaxPageSize()
        {
            // Arrange
            var originalMaxPageSize = Constants.MaxPageSize;
            Constants.MaxPageSize = 2;

            var owner = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test"
            };

            var timeEntries = new TimeEntry[]
            {
                new TimeEntry()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTimeOffset.Now.AddDays(-1),
                    Duration = 0.5,
                    Note = "My first time entry",
                    Owner = owner,
                    OwnerId = owner.Id
                },
                new TimeEntry()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTimeOffset.Now,
                    Duration = 11,
                    Note = "My second time entry",
                    Owner = owner,
                    OwnerId = owner.Id
                },
                new TimeEntry()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTimeOffset.Now.AddDays(-2),
                    Duration = 7,
                    Note = "My third time entry",
                    Owner = owner,
                    OwnerId = owner.Id
                }
            };

            var timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
            timeEntryRepositoryMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync()).ReturnsAsync(owner);

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepositoryMock.Object),
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object)
            );
            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = true,
                PageSize = 10
            };

            // Act
            var result = await timeEntryService.GetTimeEntries(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Results.Length);

            // Rollback max page size setting
            Constants.MaxPageSize = originalMaxPageSize;
        }

        [Theory(DisplayName = "When get time entries with paging option in query string, result must be paged.")]
        [InlineData("?$skip=0&$top=1&", 1, 1)]
        [InlineData("?$skip=0&$top=2&", 2, 2)]
        [InlineData("?$skip=0&$top=3&", 3, 3)]
        [InlineData("?$skip=1&$top=2&", 2, 2)]
        [InlineData("?$skip=1&$top=1&", 1, 1)]
        [InlineData("?$skip=2&$top=2&", 2, 2)]
        [InlineData("?$skip=3&$top=3&", 3, 1)]
        [InlineData("?$skip=3&$top=1&", 1, 1)]
        [InlineData("?$skip=4&$top=1&", 1, 0)]
        public async Task GetTimeEntries_HasPagingOption_PagedResult(string queryString, int top, int expectedLength)
        {
            // Arrange
            var currentUser = new User() { Id = Guid.NewGuid().ToString() };
            var timeEntries = new[]
            {
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id },
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id },
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id },
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id }
            };
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync()).ReturnsAsync(currentUser);

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object),
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object)
            );

            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = true,
                PageSize = top,
                QueryString = queryString
            };

            // Act
            var result = await timeEntryService.GetTimeEntries(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLength, result.Results.Length);
        }

        [Theory(DisplayName = "When get time entries with valid filter, it returns correct data set.")]
        [InlineData("?$filter=Date gt datetime'2017-03-04T17:12'", 2)]
        [InlineData("?$filter=Date gt datetime'2017-01-02T17:12' and Date lt datetime'2017-03-05T00:00'", 2)]
        public async Task GetTimeEntries_ValidFilter_ReturnCorrectData(string queryString, int expectedCount)
        {
            // Arrange
            var currentUser = new User() { Id = Guid.NewGuid().ToString() };
            var timeEntries = new[]
            {
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id,
                    Date = new DateTimeOffset(2017, 3, 1, 0, 0, 0, TimeSpan.FromHours(7)) },
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id,
                    Date = new DateTimeOffset(2017, 3, 5, 0, 10, 0, TimeSpan.FromHours(7)) },
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id,
                    Date = new DateTimeOffset(2017, 2, 25, 0, 0, 0, TimeSpan.FromHours(7)) },
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id,
                    Date = new DateTimeOffset(2017, 3, 6, 0, 0, 0, TimeSpan.FromHours(7)) }
            };

            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());


            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync()).ReturnsAsync(currentUser);

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object),
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object)
            );

            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = true,
                QueryString = queryString
            };

            // Act
            var result = await timeEntryService.GetTimeEntries(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Results.Length);
        }


        [Fact(DisplayName = "When get time entries, result is sorted by Date by default.")]
        public async Task GetTimeEntries_Success_SortByDateByDefault()
        {
            // Arrange
            var currentOwner = new User() { Id = Guid.NewGuid().ToString() };
            var timeEntries = new[]
            {
                new TimeEntry() { Id = Guid.NewGuid(), Note = "-3", Owner = currentOwner, OwnerId = currentOwner.Id, Date = DateTimeOffset.UtcNow.AddDays(-3) },
                new TimeEntry() { Id = Guid.NewGuid(), Note = "-2", Owner = currentOwner, OwnerId = currentOwner.Id, Date = DateTimeOffset.UtcNow.AddDays(-2) },
                new TimeEntry() { Id = Guid.NewGuid(), Note = "1", Owner = currentOwner, OwnerId = currentOwner.Id, Date = DateTimeOffset.UtcNow.AddDays(1) },
                new TimeEntry() { Id = Guid.NewGuid(), Note = "3", Owner = currentOwner, OwnerId = currentOwner.Id, Date = DateTimeOffset.UtcNow.AddDays(3) },
                new TimeEntry() { Id = Guid.NewGuid(), Note = "2", Owner = currentOwner, OwnerId = currentOwner.Id, Date = DateTimeOffset.UtcNow.AddDays(2) },
                new TimeEntry() { Id = Guid.NewGuid(), Note = "-1", Owner = currentOwner, OwnerId = currentOwner.Id, Date = DateTimeOffset.UtcNow.AddDays(-1) },
                new TimeEntry() { Id = Guid.NewGuid(), Note = "0", Owner = currentOwner, OwnerId = currentOwner.Id, Date = DateTimeOffset.UtcNow }
            };
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync()).ReturnsAsync(currentOwner);

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object),
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object)
            );
            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = true
            };
            var lastMinDate = DateTimeOffset.MinValue;

            // Act
            var result = await timeEntryService.GetTimeEntries(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(timeEntries.Length, result.Results.Length);
            foreach (var r in result.Results)
            {
                Assert.True(r.Date > lastMinDate);
                lastMinDate = r.Date;
            }
        }


        [Fact(DisplayName = "When get all time entries without authorization, it throws exception")]
        public async Task GetAllTimeEntries_Unauthorized_ThrowException()
        {
            // Arrange
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(auth => auth
                    .AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<Type>()))
                .Throws<AuthorizationException>();

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object)
            );
            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = true
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => timeEntryService.GetTimeEntries(query)
            );
            authorizationServiceMock.Verify(auth =>
                auth.AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), Operation.ReadAll, typeof(TimeEntry))
            );
        }


        [Fact(DisplayName = "When get all time entries with authorization, it returns time entries from all users.")]
        public async Task GetAllTimeEntries_Authorized_ReturnEntriesFromAllUsers()
        {
            // Arrange
            var owner = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test"
            };
            var anotherOwner = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Another test user"
            };

            var timeEntries = new TimeEntry[]
            {
                new TimeEntry()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTimeOffset.Now.AddDays(-1),
                    Duration = 0.5,
                    Note = "My first time entry",
                    Owner = owner,
                    OwnerId = owner.Id
                },
                new TimeEntry()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTimeOffset.Now,
                    Duration = 11,
                    Note = "My second time entry",
                    Owner = anotherOwner,
                    OwnerId = owner.Id
                },
                new TimeEntry()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTimeOffset.Now.AddDays(-2),
                    Duration = 7,
                    Note = "My third time entry",
                    Owner = owner,
                    OwnerId = owner.Id
                }
            };
            var timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
            timeEntryRepositoryMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock.SetReturnsDefault<Task>(Task.FromResult(0));

            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync()).ReturnsAsync(owner);

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object),
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepositoryMock.Object),
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object)
            );

            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = true
            };

            // Act
            var result = await timeEntryService.GetTimeEntries(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(timeEntries.Length, result.Results.Length);
        }

        [Fact(DisplayName = "When get time entries of mine, it returns my time entries only.")]
        public async Task GetTimeEntry_MineOnly_ReturnMineOnly()
        {
            // Arrange
            var currentUser = new User() { Id = Guid.NewGuid().ToString() };
            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync())
                .ReturnsAsync(currentUser);

            var anotherUser = new User() { Id = Guid.NewGuid().ToString() };

            var timeEntries = new[]
            {
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id },
                new TimeEntry() { Id = Guid.NewGuid(), Owner = anotherUser, OwnerId = anotherUser.Id },
                new TimeEntry() { Id = Guid.NewGuid(), Owner = currentUser, OwnerId = currentUser.Id },
            };

            var timeEntryRespositoryMock = new Mock<ITimeEntryRepository>();
            timeEntryRespositoryMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object),
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRespositoryMock.Object)
            );

            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = false
            };

            // Act
            var result = await timeEntryService.GetTimeEntries(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Results.Length);
            Assert.All(result.Results, (r) =>
            {
                Assert.NotNull(r);
                Assert.Equal(currentUser.Id, r.OwnerId);
            });
        }


        [Theory(DisplayName = "When get time entries with invalid query string, it throws exception.")]
        [InlineData("?$filter=invalid filter")]
        [InlineData("$filter=Date gt datetime`2017-01-01T00:00`")] // missing ?
        public async Task GetTimeEntries_InvalidQueryString_ThrowException(string queryString)
        {
            // Arrange
            var timeEntryService = InstantiateTimeEntryService();
            var query = new GetTimeEntryQuery()
            {
                QueryString = queryString
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => timeEntryService.GetTimeEntries(query)
            );
        }

        #endregion Get time entries

        #region Upsert time entry

        [Fact(DisplayName = "When upsert a time entry, it checks for resource type authorization.")]
        public async Task UpsertTimeEntry_CheckResourceTypeAuthorization()
        {
            // Arrange
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(auth => auth.AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<Type>()))
                .Throws<AuthorizationException>();

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object)
            );
            var command = new UpsertTimeEntryCommand()
            {
                Id = Guid.NewGuid(),
                Date = DateTimeOffset.Now,
                Duration = 2,
                Note = "Test",
                OwnerId = Guid.NewGuid().ToString()
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => timeEntryService.UpsertTimeEntry(command)
            );
            authorizationServiceMock.Verify(
                auth => auth.AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), Operation.Upsert, typeof(TimeEntry))
            );
        }

        [Fact(DisplayName = "When upsert a time entry that is new, it checks for resource authorization.")]
        public async Task UpsertTimeEntry_NewTimeEntry_CheckResourceAuthorization()
        {
            // Arrange
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock
                .Setup(r => r.GetTimeEntryById(It.IsAny<Guid>()))
                .ReturnsAsync((TimeEntry)null);

            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(auth => auth.AuthorizeResource(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<object>()))
                .Throws<AuthorizationException>();

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object),
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );
            
            var timeEntryId = Guid.NewGuid();
            var command = new UpsertTimeEntryCommand()
            {
                Id = timeEntryId,
                Date = DateTimeOffset.Now,
                Duration = 1, 
                Note = "Test",
                OwnerId = Guid.NewGuid().ToString()
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => timeEntryService.UpsertTimeEntry(command)
            );

            authorizationServiceMock.Verify(
                auth => auth.AuthorizeResource(It.IsAny<ClaimsPrincipal>(), Operation.Create, It.IsAny<TimeEntry>())
            );
        }

        [Fact(DisplayName = "When upsert a time entry that exists, it checks for resource authorization.")]
        public async Task UpsertTimeEntry_ExistingTimeEntry_CheckResourceAuthorization()
        {
            // Arrange
            var timeEntryId = Guid.NewGuid();
            var ownerId = Guid.NewGuid().ToString();

            var existingTimeEntry = new TimeEntry()
            {
                Id = timeEntryId,
                OwnerId = ownerId,
                Date = DateTimeOffset.Now,
                Duration = 0.5,
                Note = "Original"
            };

            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock
                .Setup(r => r.GetTimeEntryById(It.IsAny<Guid>()))
                .ReturnsAsync(existingTimeEntry);

            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(auth => auth.AuthorizeResource(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<object>()))
                .Throws<AuthorizationException>();

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object),
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );

            var command = new UpsertTimeEntryCommand()
            {
                Id = timeEntryId,
                Date = DateTimeOffset.Now,
                Duration = 1,
                Note = "Test",
                OwnerId = ownerId
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => timeEntryService.UpsertTimeEntry(command)
            );

            authorizationServiceMock.Verify(
                auth => auth.AuthorizeResource(It.IsAny<ClaimsPrincipal>(), Operation.Update, existingTimeEntry)
            );
        }

        [Fact(DisplayName = "When upsert a time entry that is new, it creates new time entry.")]
        public async Task UpsertTimeEntry_New_CreateNew()
        {
            // Arrange
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock
                .Setup(r => r.GetTimeEntryById(It.IsAny<Guid>()))
                .ReturnsAsync((TimeEntry)null);

            TimeEntry passedTimeEntry = null;
            timeEntryRepoMock
                .Setup(r => r.CreateTimeEntry(It.IsAny<TimeEntry>()))
                .Callback<TimeEntry>(te => passedTimeEntry = te)
                .ReturnsAsync(passedTimeEntry);

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );

            var timeEntryId = Guid.NewGuid();
            var command = new UpsertTimeEntryCommand()
            {
                Id = timeEntryId,
                Date = DateTimeOffset.Now,
                Duration = 1,
                Note = "Test",
                OwnerId = Guid.NewGuid().ToString()
            };

            // Act
            await timeEntryService.UpsertTimeEntry(command);

            // Assert
            timeEntryRepoMock.Verify(r => r.GetTimeEntryById(timeEntryId));
            Assert.NotNull(passedTimeEntry);

            Assert.Equal(command.Id, passedTimeEntry.Id);
            Assert.Equal(command.Date, passedTimeEntry.Date);
            Assert.Equal(command.Duration, passedTimeEntry.Duration);
            Assert.Equal(command.Note, passedTimeEntry.Note);
            Assert.Equal(command.OwnerId, passedTimeEntry.OwnerId);
        }


        [Fact(DisplayName = "When upsert a time entry that exists, it updates the current that time entry.")]
        public async Task UpsertTimeEntry_Existing_Update()
        {
            // Arrange
            var timeEntryId = Guid.NewGuid();
            var timeEntry = new TimeEntry()
            {
                Id = timeEntryId,
                Date = DateTimeOffset.Now,
                Duration = 0.5,
                Note = "original note",
                OwnerId = Guid.NewGuid().ToString()
            };

            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock
                .Setup(r => r.GetTimeEntryById(It.IsAny<Guid>()))
                .ReturnsAsync(timeEntry);

            TimeEntry passedTimeEntry = null;
            timeEntryRepoMock
                .Setup(r => r.UpdateTimeEntry(It.IsAny<TimeEntry>()))
                .Callback<TimeEntry>(te => passedTimeEntry = te)
                .ReturnsAsync(passedTimeEntry);

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );

            var command = new UpsertTimeEntryCommand()
            {
                Id = timeEntryId,
                Date = DateTimeOffset.Now,
                Duration = 1,
                Note = "Test",
                OwnerId = Guid.NewGuid().ToString()
            };

            // Act
            await timeEntryService.UpsertTimeEntry(command);

            // Assert
            timeEntryRepoMock.Verify(r => r.GetTimeEntryById(timeEntryId));
            Assert.NotNull(passedTimeEntry);

            Assert.Equal(command.Id, passedTimeEntry.Id);
            Assert.Equal(command.Date, passedTimeEntry.Date);
            Assert.Equal(command.Duration, passedTimeEntry.Duration);
            Assert.Equal(command.Note, passedTimeEntry.Note);
            Assert.Equal(command.OwnerId, passedTimeEntry.OwnerId);
        }

        [Theory(DisplayName = "When upsert a time entry with invalid data, it thows validation exception.")]
        [InlineData(0.2, null, "d4d643fc-1484-4d0b-b5c3-47a983be8217", true)]
        [InlineData(10, "", "e8cef9af-60b0-4623-983d-8f1171b03963", true)]
        [InlineData(-1, "Negative duration", "e8cef9af-60b0-4623-983d-8f1171b03963", true)]
        [InlineData(24.01, "More than 1 day", "e8cef9af-60b0-4623-983d-8f1171b03963", true)]
        [InlineData(22, "Empty owner ID", "00000000-0000-0000-0000-000000000000", false)]
        [InlineData(3, "Not existing owner", "ea5bf791-b4a4-4b87-a47a-38c9f7cbd56d", false)]
        public async Task UpsertTimeEntry_InvalidData_ThrowValidationException(double duration, string note, string ownerId, bool ownerExists)
        {
            // Arrange
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(ownerExists ? new User() : null);
            var validationService = new ValidationService(
                new UpsertTimeEntryValidator(userRepoMock.Object)
            );

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(IValidationService), validationService)
            );
            var command = new UpsertTimeEntryCommand()
            {
                Id = Guid.NewGuid(),
                Date = DateTimeOffset.Now,
                Duration = duration,
                Note = note,
                OwnerId = ownerId
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => timeEntryService.UpsertTimeEntry(command)
            );
        }

        #endregion Upsert time entry

        #region Delete time entry

        [Fact(DisplayName = "When delete a time entry that does not exist, it does nothing.")]
        public async Task DeleteTimeEntry_NotExist_DoNothing()
        {
            // Arrange
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock
                .Setup(repo => repo.GetTimeEntryById(It.IsAny<Guid>()))
                .ReturnsAsync((TimeEntry)null);

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );
            var command = new DeleteTimeEntryCommand()
            {
                TimeEntryId = Guid.NewGuid()
            };

            // Act
            await timeEntryService.DeleteTimeEntry(command);

            // Assert
            timeEntryRepoMock.Verify(
                repo => repo.DeleteTimeEntry(It.IsAny<TimeEntry>()),
                Times.Never
            );
        }

        [Theory(DisplayName = "When delete a time entry with invalid command, it throws exception.")]
        [InlineData("", true)]
        [InlineData("00000000-0000-0000-0000-000000000000", false)]
        public async Task DeleteTimeEntry_InvalidCommand_ThrowException(string id, bool passNullCommand)
        {
            // Arrange
            DeleteTimeEntryCommand command = null;
            if (!passNullCommand)
            {
                command = new DeleteTimeEntryCommand()
                {
                    TimeEntryId = Guid.Parse(id)
                };
            }

            var timeEntryService = InstantiateTimeEntryService();

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => timeEntryService.DeleteTimeEntry(command)
            );
        }

        [Fact(DisplayName = "When delete a time entry successfully, it deletes time entry in persistence.")]
        public async Task DeleteTimeEntry_Success_DeleteInPersistence()
        {
            // Arrange
            var timeEntryId = Guid.NewGuid();
            var existingTimeEntry = new TimeEntry() { Id = timeEntryId };

            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(repo => repo.GetTimeEntryById(It.IsAny<Guid>()))
                .ReturnsAsync(existingTimeEntry);

            timeEntryRepoMock.Setup(repo => repo.DeleteTimeEntry(It.IsAny<TimeEntry>()))
                .Returns(Task.FromResult(0));

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );
            var command = new DeleteTimeEntryCommand()
            {
                TimeEntryId = timeEntryId
            };

            // Act
            await timeEntryService.DeleteTimeEntry(command);

            // Assert
            timeEntryRepoMock.Verify(repo => repo.DeleteTimeEntry(existingTimeEntry));
        }

        [Fact(DisplayName = "When delete a time entry, it checks for resource type authorization.")]
        public async Task DeleteTimeEntry_CheckResourceTypeAuthorization()
        {
            // Arrange
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();

            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(auth => auth.AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<Type>()))
                .Throws<AuthorizationException>();

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object),
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object)
            );

            var command = new DeleteTimeEntryCommand()
            {
                TimeEntryId = Guid.NewGuid()
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => timeEntryService.DeleteTimeEntry(command)
            );
            timeEntryRepoMock.Verify(repo => repo.DeleteTimeEntry(It.IsAny<TimeEntry>()), Times.Never);
            authorizationServiceMock.Verify(
                auth => auth.AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), Operation.Delete, typeof(TimeEntry))
            );
        }

        [Fact(DisplayName = "When delete a time entry, it checks for resource authorization.")]
        public async Task DeleteTimeEntry_CheckAuthorization()
        {
            // Arrange
            var timeEntry = new TimeEntry() { Id = Guid.NewGuid() };

            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(repo => repo.GetTimeEntryById(It.IsAny<Guid>()))
                .ReturnsAsync(timeEntry);

            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(auth => auth.AuthorizeResource(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<object>()))
                .Throws<AuthorizationException>();

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object),
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object)
            );
            var command = new DeleteTimeEntryCommand()
            {
                TimeEntryId = timeEntry.Id
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => timeEntryService.DeleteTimeEntry(command)
            );
            timeEntryRepoMock.Verify(repo => repo.DeleteTimeEntry(It.IsAny<TimeEntry>()), Times.Never);
            authorizationServiceMock.Verify(
                auth => auth.AuthorizeResource(It.IsAny<ClaimsPrincipal>(), Operation.Delete, timeEntry)
            );
        }

        #endregion Delete time entry

        #region Generate summary report

        [Theory(DisplayName = "When generate summary report with invalid filter, it throws exception.")]
        [InlineData("?$filter=invalid filter")]
        [InlineData("$filter=Date gt datetime`2017-01-01T00:00`")] // missing ?
        [InlineData("$filter=Date gt datetime`2017-01-01T00:00:00.21Z`")] // invalid datetime format
        public async Task GenerateSummaryReport_InvalidFilter_ThrowException(string queryString)
        {
            // Arrange
            var query = new GenerateTimeEntrySummaryReportQuery()
            {
                QueryString = queryString
            };

            var timeEntryService = InstantiateTimeEntryService();

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => timeEntryService.GenerateSummaryReport(query)
            );
        }

        [Fact(DisplayName = "When generate report with IncludeAll enabled and authorized, it aggregates time entries from all users.")]
        public async Task GenerateSummaryReport_AuthorizedGetAll_AggregateDataFromAllUsers()
        {
            // Arrange
            var me = new User() { Id = Guid.NewGuid().ToString() };
            var anotherUser = new User() { Id = Guid.NewGuid().ToString() };

            var timeEntries = new[]
            {
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Owner = anotherUser, OwnerId = anotherUser.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Owner = anotherUser, OwnerId = anotherUser.Id  }
            };
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );
            var query = new GenerateTimeEntrySummaryReportQuery()
            {
                IncludeTimeEntriesOfAllUsers = true
            };

            // Act
            var result = await timeEntryService.GenerateSummaryReport(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            var uniqueOwnerIds = result.Select(r => r.OwnerId).Distinct();
            Assert.Equal(2, uniqueOwnerIds.Count());
            Assert.All(
                uniqueOwnerIds,
                id => Assert.True(id == me.Id || id == anotherUser.Id)
            );
        }

        [Theory(DisplayName = "When generate report without resource type authorization, it throws exception.")]
        [InlineData(false, Operation.Read)]
        [InlineData(true, Operation.ReadAll)]
        public async Task GenerateSummaryReport_UnauthorizedResourceType_ThrowException(bool includeAll, Operation expectedOperation)
        {
            // Arrange
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(auth => auth.AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<Type>()))
                .Throws<AuthorizationException>();

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object)
            );

            var query = new GenerateTimeEntrySummaryReportQuery()
            {
                IncludeTimeEntriesOfAllUsers = includeAll
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => timeEntryService.GenerateSummaryReport(query)
            );
            authorizationServiceMock.Verify(
                auth => auth.AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), expectedOperation, typeof(TimeEntry))
            );
        }

        [Fact(DisplayName = "When generate report, it aggregates data correctly.")]
        public async Task GenerateSummaryReport_AggregateCorrectly()
        {
            // Arrange
            var me = new User() { Id = Guid.NewGuid().ToString() };
            var anotherUser = new User() { Id = Guid.NewGuid().ToString() };

            var timeEntries = new[]
            {
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Note = "Note 1", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Note = "Note 2", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Note = "Note 3", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Note = "Note 4", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Note = "Note 5", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Note = "Note 1", Owner = anotherUser, OwnerId = anotherUser.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = DateTime.Now, Duration = 1, Note = "Note 2", Owner = anotherUser, OwnerId = anotherUser.Id  }
            };
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );

            var query = new GenerateTimeEntrySummaryReportQuery()
            {
                IncludeTimeEntriesOfAllUsers = true
            };

            var expectedNoteMe = new[] { "Note 1", "Note 2", "Note 3", "Note 4", "Note 5" };
            var expectedNoteAnotherUser = new[] { "Note 1", "Note 2" };

            var expectedTotalTimeMe = 5;
            var expectedTotalTimeAnotherUser = 2;

            // Act
            var result = await timeEntryService.GenerateSummaryReport(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.All(result, r =>
            {
                Assert.True(r.OwnerId == me.Id || r.OwnerId == anotherUser.Id);
                Assert.True(DateTimeOffset.Now.Date == r.Date);

                if (r.OwnerId == me.Id)
                {
                    Assert.Equal(expectedTotalTimeMe, r.TotalTime);
                    Assert.Equal(expectedNoteMe, r.Notes);
                }
                else
                {
                    Assert.Equal(expectedTotalTimeAnotherUser, r.TotalTime);
                    Assert.Equal(expectedNoteAnotherUser, r.Notes);
                }
            });
        }

        [Fact(DisplayName = "When generate report, result is sorted by date by default")]
        public async Task GenerateSummaryReport_SortByDateByDefault()
        {
            // Arrange
            var me = new User() { Id = Guid.NewGuid().ToString() };

            var timeEntries = new[]
            {
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 1, 4, 0, 0, TimeSpan.FromHours(7)), Duration = 1, Note = "Note 4", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 1, 5, 0, 0, TimeSpan.FromHours(7)), Duration = 1, Note = "Note 5", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 1, 2, 0, 0, TimeSpan.FromHours(7)), Duration = 1, Note = "Note 2", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 1, 3, 0, 0, TimeSpan.FromHours(7)), Duration = 1, Note = "Note 3", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 1, 1, 0, 0, TimeSpan.FromHours(7)), Duration = 1, Note = "Note 1", Owner = me, OwnerId = me.Id  },
            };
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );

            var query = new GenerateTimeEntrySummaryReportQuery()
            {
                IncludeTimeEntriesOfAllUsers = true
            };

            var expectedNotes = new[] { "Note 1", "Note 2", "Note 3", "Note 4", "Note 5" };

            // Act
            var result = await timeEntryService.GenerateSummaryReport(query);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            var notes = result[0].Notes;
            Assert.NotEmpty(notes);
            Assert.Equal(expectedNotes, notes);

            for (var i = 0; i < notes.Length; ++i)
            {
                Assert.Equal(expectedNotes[i], notes[i]);
            }
        }

        [Fact(DisplayName = "When generate report, it pre-computes IsUnderPreferredWorkingHoursPerDay.")]
        public async Task GenerateSummaryReport_ComputeIsUnderPreferredWorkingHoursPerDay()
        {
            var me = new User() { Id = Guid.NewGuid().ToString(), PreferredWorkingHourPerDay = 8 };

            var timeEntries = new[]
            {
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 1, 4, 0, 0, TimeSpan.FromHours(7)), Duration = 4, Note = "Note 4", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 1, 4, 0, 0, TimeSpan.FromHours(7)), Duration = 5, Note = "Note 5", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 2, 5, 0, 0, TimeSpan.FromHours(7)), Duration = 3, Note = "Note 2", Owner = me, OwnerId = me.Id  },
                new TimeEntry() { Id = Guid.NewGuid(), Date = new DateTimeOffset(2017, 1, 2, 5, 0, 0, TimeSpan.FromHours(7)), Duration = 2, Note = "Note 3", Owner = me, OwnerId = me.Id  }
            };
            var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            timeEntryRepoMock.Setup(r => r.GetTimeEntries())
                .Returns(timeEntries.AsQueryable());

            var timeEntryService = InstantiateTimeEntryService(
                new Tuple<Type, object>(typeof(ITimeEntryRepository), timeEntryRepoMock.Object)
            );

            var query = new GenerateTimeEntrySummaryReportQuery()
            {
                IncludeTimeEntriesOfAllUsers = true
            };

            // Act
            var result = await timeEntryService.GenerateSummaryReport(query);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.False(result[0].IsUnderPreferredWorkingHoursPerDay); // 9 > 8
            Assert.True(result[1].IsUnderPreferredWorkingHoursPerDay); // 5 < 8
        }

        #endregion Generate summary report
    }
}
