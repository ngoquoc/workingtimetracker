using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using WorkingTimeTracker.Core.Services;
using WorkingTimeTracker.Core.Entities;
using System.Collections.Generic;

namespace WorkingTimeTracker.Implementations.Services
{
    public interface IAspNetIdentityUserManager : IUserManager
    {
        Task<IdentityResult> CreateAsync(IdentityUser identity, string password);

        Task<IdentityUser> FindByEmailAsync(string email);

        Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    }

    public class AspNetIdentityUserManager : IAspNetIdentityUserManager
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AspNetIdentityUserManager(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        Task<IdentityResult> IAspNetIdentityUserManager.ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            return this.userManager.ChangePasswordAsync(userId, oldPassword, newPassword);
        }

        Task IUserManager.CreateAsync(User user)
        {
            var identity = new IdentityUser(user.Email)
            {
                Id = user.Id,
                Email = user.Email
            };
            return userManager.CreateAsync(identity, Core.Constants.DEFAULT_PASSWORD);
        }

        Task<IdentityResult> IAspNetIdentityUserManager.CreateAsync(IdentityUser identity, string password)
        {
            return this.userManager.CreateAsync(identity, password);
        }

        Task<IdentityUser> IAspNetIdentityUserManager.FindByEmailAsync(string email)
        {
            return userManager.FindByEmailAsync(email);
        }

        async Task IUserManager.UpdateUserRoles(string userId, string[] roles)
        {
            var currentRoles = userManager.GetRoles(userId);
            await userManager.RemoveFromRolesAsync(userId, currentRoles.ToArray());
            await userManager.AddToRolesAsync(userId, roles);
        }

        async Task IUserManager.RemoveUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                await userManager.DeleteAsync(user);
            }

        }

        async Task<string[]> IUserManager.GetRoles(string userId)
        {
            var roles = await userManager.GetRolesAsync(userId);
            return roles.ToArray();
        }

        Task<Dictionary<string, int>> IUserManager.CountUsersInRoles()
        {
            return Task.FromResult(
                roleManager.Roles.ToDictionary(
                    keySelector: (role) => role.Name,
                    elementSelector: (role) => role.Users.Count
            ));
        }
    }
}
