using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using WorkingTimeTracker.Core.Services;
using Autofac;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WorkingTimeTracker.Web.Middlewares
{
    public class IdentityRefreshMiddleware : OwinMiddleware
    {
        private readonly UserManager<IdentityUser> userManager;

        public IdentityRefreshMiddleware(OwinMiddleware next, UserManager<IdentityUser> userManager) 
            : base(next)
        {
            this.userManager = userManager;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var currentPrincipal = context.Request.User as ClaimsPrincipal;
            var claims = currentPrincipal.Claims.Where(c => c.Type != ClaimTypes.Role).ToList();

            if (currentPrincipal != null)
            {
                var idClaim = claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (idClaim != null)
                {
                    var userId = idClaim.Value;

                    var roles = await userManager.GetRolesAsync(userId);

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var identity = new ClaimsIdentity(claims);
                    var principal = new ClaimsPrincipal(identity);

                    context.Request.User = principal;
                    System.Threading.Thread.CurrentPrincipal = principal;
                    HttpContext.Current.User = principal;
                }
            }

            await Next.Invoke(context);
        }
    }
}