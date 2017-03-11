using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Core.Services;

namespace WorkingTimeTracker.Implementations.Services
{
    public class ThreadPrincipalCurrentUserResolver : ICurrentUserResolver
    {
        private readonly IUserRepository userRepository;

        public ThreadPrincipalCurrentUserResolver(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        Task<User> ICurrentUserResolver.ResolveAsync()
        {
            var principal = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (principal == null)
            {
                return null;
            }

            var idClaim = principal.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (idClaim == null)
            {
                return null;
            }

            return userRepository.GetUserById(idClaim.Value);
        }

        Task<ClaimsPrincipal> ICurrentUserResolver.ResolveCurrentClaimsPrincipalAsync()
        {
            return Task.FromResult(Thread.CurrentPrincipal as ClaimsPrincipal);
        }
    }
}
