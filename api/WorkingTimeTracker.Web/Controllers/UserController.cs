using System.Linq;
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

namespace WorkingTimeTracker.Web.Controllers
{
    [AuthorizeRoles(Roles = Constants.ROLE_AMIN + ", " + Constants.ROLE_USER_MANGER)]
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Get(bool excludeMe = true)
        {
            var queryString = HttpContext.Current.Request.QueryString;

            var rawQueryString = HttpUtility.UrlDecode(Request.RequestUri.Query);
            var orderBy = queryString["$orderby"];

            var query = new GetUsersQuery()
            {
                ExcludeMe = excludeMe,
                QueryString = rawQueryString,
                OrderBy = orderBy
            };

            var pageSize = queryString["$top"];
            int pageSizeInt;
            if (int.TryParse(pageSize, out pageSizeInt))
            {
                query.Top = pageSizeInt;
            }

            try
            {
                var result = await userService.GetUsersWithRoles(query);
                return Ok(result);
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
        [Route("{id}")]
        public async Task<IHttpActionResult> Get(string id)
        {
            var query = new GetUsersQuery()
            {
                QueryString = $"?$filter=Id='{id}'"
            };

            try
            {
                var users = await userService.GetUsersWithRoles(query);
                if (!users.Results.Any())
                {
                    return NotFound();
                }
                else
                {
                    return Ok(users.Results.First());
                }
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
        [Route("")]
        public async Task<IHttpActionResult> Put(UpsertUserCommand command)
        {
            try
            {
                await userService.UpsertUser(command);
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
        public async Task<IHttpActionResult> Put([FromUri]string id, [FromBody]UpsertUserCommand command)
        {
            try
            {
                if (command != null)
                {
                    command.Id = id;
                }

                await userService.UpsertUser(command);
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
        public async Task<IHttpActionResult> Delete(DeleteUserCommand command)
        {
            try
            {
                await userService.DeleteUser(command);
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
        public async Task<IHttpActionResult> Delete([FromUri]string id, [FromBody]DeleteUserCommand command)
        {
            try
            {
                if (command == null)
                {
                    command = new DeleteUserCommand();
                }

                command.UserId = id;
                await userService.DeleteUser(command);
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
    }
}