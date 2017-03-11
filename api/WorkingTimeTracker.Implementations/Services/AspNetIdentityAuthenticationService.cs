using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using WorkingTimeTracker.Core;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Core.Services;

namespace WorkingTimeTracker.Implementations.Services
{
    public class AspNetIdentityAuthenticationService : IAuthenticationService
    {
        private readonly IValidationService validationService;
        private readonly IAspNetIdentityUserManager userManager;
        private readonly ISignInManager signinManager;
        private readonly IUserRepository userRepository;
        private readonly ICurrentUserResolver currentUserResolver;

        public AspNetIdentityAuthenticationService(IValidationService validationService,
            IAspNetIdentityUserManager userManager,
            ISignInManager signinManager,
            IUserRepository userRepository,
            ICurrentUserResolver currentUserResolver)
        {
            this.validationService = validationService;
            this.userManager = userManager;
            this.signinManager = signinManager;
            this.userRepository = userRepository;
            this.currentUserResolver = currentUserResolver;
        }

        async Task IAuthenticationService.ChangePassword(ChangePasswordCommand command)
        {
            await validationService.Validate(command);

            var currentUser = await currentUserResolver.ResolveAsync();
            if (currentUser == null)
            {
                throw new ChangePasswordException("Unauthorized.");
            }

            var result = await userManager
                .ChangePasswordAsync(currentUser.Id, command.OldPassword, command.NewPassword);
            if (!result.Succeeded)
            {
                throw new ChangePasswordException(string.Join("\n", result.Errors));
            }
        }

        async Task IAuthenticationService.Login(string userName, string password)
        {
            var result = await signinManager.PasswordSignInAsync(userName, password, true, false);

            if (result != SignInStatus.Success)
            {
                string message = "Failed to login.";
                if (result == SignInStatus.Failure)
                {
                    message = "Bad user name or password combination.";
                }
                else if (result == SignInStatus.LockedOut)
                {
                    message = "Account has been locked.";
                }
                throw new LoginException(message);
            }
        }

        async Task IAuthenticationService.Register(RegisterCommand command)
        {
            await validationService.Validate(command);

            var identityUser = new IdentityUser()
            {
                Email = command.Email,
                UserName = command.Email
            };

            var result = await userManager.CreateAsync(identityUser, command.Password);

            if (!result.Succeeded)
            {
                throw new RegistrationException(string.Join("\n", result.Errors));
            }

            var createdUser = await userManager.FindByEmailAsync(command.Email);

            await userManager.UpdateUserRoles(createdUser.Id, new[] { Constants.ROLE_USER });
            await userRepository.CreateUser(new User()
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                Name = command.Name
            });
        }
    }
}
