using System.Threading.Tasks;
using System.Web.Http;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Services;
using WorkingTimeTracker.Core.Validators;
using WorkingTimeTracker.Web.Attributes;

namespace WorkingTimeTracker.Web.Controllers
{
    [AuthorizeRoles]
    [RoutePrefix("api/auth")]
    public class AuthenticationController : ApiController
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IUserService userService;

        public AuthenticationController(IAuthenticationService authenticationService,
            IUserService userService)
        {
            this.authenticationService = authenticationService;
            this.userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<IHttpActionResult> Register(RegisterCommand command)
        {
            try
            {
                await authenticationService.Register(command);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (RegistrationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost]
        [Route("changePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordCommand command)
        {
            try
            {
                await authenticationService.ChangePassword(command);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ChangePasswordException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet]
        [Route("me")]
        public async Task<IHttpActionResult> GetCurrentUser()
        {
            var currentUser = await userService.GetCurrentUserWithRoles();
            return Ok(new
            {
                user = currentUser
            });
        }

        [HttpPost]
        [Route("me/preferrences")]
        public async Task<IHttpActionResult> UpdatePreferrences(UpdateCurrentUserSettingsCommand command)
        {
            try
            {
                await userService.UpdateCurrentUserSettings(command);
                return Ok();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
