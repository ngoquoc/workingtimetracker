using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Authorization;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Queries;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Core.Validators;

namespace WorkingTimeTracker.Core.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IValidationService validationService;
        private readonly IAuthorizationService authorizationService;
        private readonly IUserRepository userRepository;
        private readonly IUserManager userManager;
        private readonly ICurrentUserResolver currentUserResolver;
        private readonly IQueryParser queryParser;

        public UserService(IValidationService validationService,
                IAuthorizationService authorizationService,
                IUserRepository userRepository,
                IUserManager userManager,
                ICurrentUserResolver currentUserResolver,
                IQueryParser queryParser)
        {
            this.validationService = validationService;
            this.authorizationService = authorizationService;
            this.userRepository = userRepository;
            this.userManager = userManager;
            this.currentUserResolver = currentUserResolver;
            this.queryParser = queryParser;
        }

        async Task IUserService.DeleteUser(DeleteUserCommand command)
        {
            Check.NotNull(command, name: "Command");
            await validationService.Validate(command);

            var user = await userRepository.GetUserById(command.UserId);
            if (user == null)
            {
                return;
            }

            var currentPrincipal = await currentUserResolver.ResolveCurrentClaimsPrincipalAsync();
            var operation = command.ForceDelete ? Operation.ForceDelete : Operation.Delete;
            await authorizationService.AuthorizeResource(currentPrincipal, operation, user);

            try
            {
                await userRepository.DeleteUser(user, command.ForceDelete);
            }
            catch (RelationshipException ex)
            {
                throw new DeleteUserException("There are time entries associated with this user, try force delete.", ex);
            }

            await userManager.RemoveUser(user.Id);
        }

        async Task<PagedResult<UserWithRoles>> IUserService.GetUsersWithRoles(GetUsersQuery query)
        {
            Check.NotNull(query, name: "Query");

            var currentPrincipal = await currentUserResolver.ResolveCurrentClaimsPrincipalAsync();
            await authorizationService.AuthorizeResourceType(currentPrincipal, Operation.Read, typeof(UserWithRoles));

            var users = userRepository.GetUsers();

            if (query.ExcludeMe)
            {
                var currentUser = await currentUserResolver.ResolveAsync();
                users = users.Where(u => u.Id != currentUser.Id);
            }

            var pagedResult = new PagedResult<UserWithRoles>();
            if (!string.IsNullOrWhiteSpace(query.QueryString))
            {
                try
                {
                    var parseResult = await queryParser.ApplyQuery(users, query.QueryString);
                    pagedResult.TotalCount = parseResult.TotalCount;
                    users = parseResult.Results;
                }
                catch (Exception ex)
                {
                    throw new ValidationException("Invalid query string", ex);
                }
            }

            if (string.IsNullOrWhiteSpace(query.OrderBy))
            {
                users = users.OrderBy(u => u.Name);
            }

            if (query.Top > Constants.MaxPageSize)
            {
                users = users.Take(Constants.MaxPageSize);
            }

            var usersWithRoles = new List<UserWithRoles>();

            foreach (var user in users)
            {
                usersWithRoles.Add(new UserWithRoles(user)
                {
                    Roles = await userManager.GetRoles(user.Id)
                });
            }

            pagedResult.Results = usersWithRoles.ToArray();
            return pagedResult;
        }

        async Task<User> IUserService.UpsertUser(UpsertUserCommand command)
        {
            Check.NotNull(command, "Command");
            await validationService.Validate(command);
            var currentPrincipal = await currentUserResolver.ResolveCurrentClaimsPrincipalAsync();

            var user = await userRepository.GetUserById(command.Id);
            if (user != null)
            {
                await authorizationService.AuthorizeResource(currentPrincipal, Operation.Update, user);

                user.Email = command.Email;
                user.Name = command.Name;
                await userRepository.UpdateUser(user);
            }
            else
            {
                user = new User()
                {
                    Id = command.Id,
                    Email = command.Email,
                    Name = command.Name
                };

                await authorizationService.AuthorizeResource(currentPrincipal, Operation.Create, user);

                await userManager.CreateAsync(user);
                await userRepository.CreateUser(user);
            }

            if (command.Roles != null)
            {
                await userManager.UpdateUserRoles(user.Id, command.Roles);
            }

            return user;
        }

        async Task<CurrentUserData> IUserService.GetCurrentUserWithRoles()
        {
            var currentUser = await currentUserResolver.ResolveAsync();
            var roles = await userManager.GetRoles(currentUser.Id);

            return new CurrentUserData(currentUser)
            {
                Roles = roles
            };
        }

        async Task IUserService.UpdateCurrentUserSettings(UpdateCurrentUserSettingsCommand command)
        {
            Check.NotNull(command, errorMessage: "Command can not be null.");
            await validationService.Validate(command);

            var user = await currentUserResolver.ResolveAsync();
            user.Name = command.Name;
            user.PreferredWorkingHourPerDay = command.PreferredWorkingHourPerDay;

            await userRepository.UpdateUser(user);
        }
    }
}
