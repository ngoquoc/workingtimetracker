using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WorkingTimeTracker.Core.Authorization
{
    public interface IAuthorizationService
    {
        Task AuthorizeResource(ClaimsPrincipal user, Operation operation, object resource);

        Task AuthorizeResourceType(ClaimsPrincipal user, Operation operation, Type type);
    }
}
