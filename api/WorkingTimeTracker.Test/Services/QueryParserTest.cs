using System;
using System.Collections.Generic;
using System.Linq;
using LinqToQuerystring;
using WorkingTimeTracker.Core.Entities;
using Xunit;

namespace WorkingTimeTracker.Test.Services
{
    public class QueryParserTest
    {

        [Theory(DisplayName = "When parse query with $inlinecount parameter, total count is returned.")]
        [InlineData("?$top=2&$skip=1&$inlinecount=allpages")]
        public void ParseQuery_HasInlineCount_ReturnTotalCount(string queryString)
        {
            // Arrange
            var timeEntries = new TimeEntry[]
            {
                new TimeEntry() { Id = Guid.NewGuid() },
                new TimeEntry() { Id = Guid.NewGuid() },
                new TimeEntry() { Id = Guid.NewGuid() },
                new TimeEntry() { Id = Guid.NewGuid() },
                new TimeEntry() { Id = Guid.NewGuid() },
                new TimeEntry() { Id = Guid.NewGuid() }
            }
            .AsQueryable();

            // Act
            var result = timeEntries.LinqToQuerystring(typeof(TimeEntry), queryString);

            // Assert
            Assert.NotNull(result);
        }
    }
}
