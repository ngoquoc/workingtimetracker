using System.Security.Claims;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Core.Services
{
    public interface ICurrentUserResolver
    {
        Task<User> ResolveAsync();

        Task<ClaimsPrincipal> ResolveCurrentClaimsPrincipalAsync();
    }
}
