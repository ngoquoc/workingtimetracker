using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Linq;

namespace WorkingTimeTracker.Web.Attributes
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        private string[] roles;

        public AuthorizeRolesAttribute()
        {
            roles = new string[0];
        }

        public AuthorizeRolesAttribute(params string[] roles) : base()
        {
            this.roles = roles;
            Roles = string.Join(",", roles);
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
            if (principal != null
                && principal.Identity != null)
            {
                if (Roles.Length == 0)
                {
                    return true;
                }
                else
                {
                    var currentRoles = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(r => r.Value);
                    return currentRoles.Any(r => roles.Contains(r));
                }
            }

            return base.IsAuthorized(actionContext);
        }
    }
}