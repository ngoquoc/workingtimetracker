using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WorkingTimeTracker.Core;
using WorkingTimeTracker.Core.Authorization;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Queries;
using WorkingTimeTracker.Core.Services;
using WorkingTimeTracker.Core.Validators;
using WorkingTimeTracker.Web.Attributes;
using WorkingTimeTracker.Web.Templates;

namespace WorkingTimeTracker.Web.Controllers
{
    [AuthorizeRoles(Roles = Constants.ROLE_AMIN + ", " + Constants.ROLE_USER)]
    [RoutePrefix("api/timeEntry")]
    public class TimeEntryController : ApiController
    {
        private readonly TimeEntrySummaryReportTemplating timeEntrySummaryReportTemplating;
        private readonly ITimeEntryService timeEntryService;

        public TimeEntryController(TimeEntrySummaryReportTemplating timeEntrySummaryReportTemplating,
            ITimeEntryService timeEntryService)
        {
            this.timeEntrySummaryReportTemplating = timeEntrySummaryReportTemplating;
            this.timeEntryService = timeEntryService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var queryString = HttpContext.Current.Request.QueryString;

            var rawQueryString = HttpUtility.UrlDecode(Request.RequestUri.Query);
            var orderBy = queryString["$orderby"];

            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = false,
                OrderBy = orderBy,
                QueryString = rawQueryString
            };

            var pageSize = queryString["$top"];
            int pageSizeInt;
            if (int.TryParse(pageSize, out pageSizeInt))
            {
                query.PageSize = pageSizeInt;
            }

            try
            {
                var timeEntries = await timeEntryService.GetTimeEntries(query);
                return Ok(timeEntries);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AuthorizationException)
            {
                return this.StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        [Route("all")]
        public async Task<IHttpActionResult> GetAll()
        {
            var queryString = HttpContext.Current.Request.QueryString;

            var rawQueryString = HttpUtility.UrlDecode(Request.RequestUri.Query);
            var orderBy = queryString["$orderby"];

            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = true,
                OrderBy = orderBy,
                QueryString = rawQueryString
            };

            var pageSize = queryString["$top"];
            int pageSizeInt;
            if (int.TryParse(pageSize, out pageSizeInt))
            {
                query.PageSize = pageSizeInt;
            }

            try
            {
                var timeEntries = await timeEntryService.GetTimeEntries(query);
                return Ok(timeEntries);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AuthorizationException)
            {
                return this.StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IHttpActionResult> Get(string id, bool includeAllUsers = false)
        {
            var query = new GetTimeEntryQuery()
            {
                IncludeAllUsers = includeAllUsers,
                QueryString = $"?$filter=Id eq guid'{id}'"
            };

            try
            {
                var timeEntries = await timeEntryService.GetTimeEntries(query);
                if (!timeEntries.Results.Any())
                {
                    return NotFound();
                }
                else
                {
                    return Ok(timeEntries.Results.First());
                }
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AuthorizationException)
            {
                return this.StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
        }

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Put(UpsertTimeEntryCommand command)
        {
            try
            {
                var upserted = await timeEntryService.UpsertTimeEntry(command);
                return Ok();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AuthorizationException)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> Put([FromUri]Guid id, [FromBody]UpsertTimeEntryCommand command)
        {
            try
            {
                if (command != null)
                {
                    command.Id = id;
                }

                var upserted = await timeEntryService.UpsertTimeEntry(command);
                return Ok();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AuthorizationException)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
        }

        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete(DeleteTimeEntryCommand command)
        {
            try
            {
                await timeEntryService.DeleteTimeEntry(command);
                return Ok();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AuthorizationException)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete([FromUri]Guid id)
        {
            try
            {
                var command = new DeleteTimeEntryCommand()
                {
                    TimeEntryId = id
                };

                await timeEntryService.DeleteTimeEntry(command);
                return Ok();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AuthorizationException)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        [Route("report")]
        public async Task<HttpResponseMessage> GenerateSummaryReport()
        {
            var query = new GenerateTimeEntrySummaryReportQuery()
            {
                IncludeTimeEntriesOfAllUsers = false,
                QueryString = HttpUtility.UrlDecode(Request.RequestUri.Query)
            };

            try
            {
                var reportItems = await timeEntryService.GenerateSummaryReport(query);

                var response = new HttpResponseMessage();
                var content = timeEntrySummaryReportTemplating.Parse(reportItems);
                
                response.Content = new StringContent(content);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

                return response;
            }
            catch (ValidationException ex)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.Message);
                return response;
            }
            catch (AuthorizationException)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                return response;
            }
        }

        [HttpGet]
        [Route("report/all")]
        public async Task<HttpResponseMessage> GenerateSummaryReportForAllUsers()
        {
            var query = new GenerateTimeEntrySummaryReportQuery()
            {
                IncludeTimeEntriesOfAllUsers = true,
                QueryString = HttpUtility.UrlDecode(Request.RequestUri.Query)
            };

            try
            {
                var reportItems = await timeEntryService.GenerateSummaryReport(query);

                var response = new HttpResponseMessage();
                var content = timeEntrySummaryReportTemplating.Parse(reportItems);

                response.Content = new StringContent(content);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

                return response;
            }
            catch (ValidationException ex)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.Message);
                return response;
            }
            catch (AuthorizationException)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                return response;
            }
        }
    }
}