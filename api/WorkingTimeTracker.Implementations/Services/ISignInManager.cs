using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace WorkingTimeTracker.Implementations.Services
{
    public interface ISignInManager
    {
        Task<SignInStatus> PasswordSignInAsync(string email, string password, bool isPersistent, bool shouldLockout);
    }

    public class SignInManager : ISignInManager
    {
        private readonly SignInManager<IdentityUser, string> signInManager;

        public SignInManager(SignInManager<IdentityUser, string> signInManager)
        {
            this.signInManager = signInManager;
        }

        Task<SignInStatus> ISignInManager.PasswordSignInAsync(string email, string password, bool isPersistent, bool shouldLockout)
        {
            return signInManager.PasswordSignInAsync(email, password, isPersistent, shouldLockout);
        }
    }
}
