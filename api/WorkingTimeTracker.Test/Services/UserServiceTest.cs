using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using WorkingTimeTracker.Core;
using WorkingTimeTracker.Core.Authorization;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Queries;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Core.Services;
using WorkingTimeTracker.Core.Services.Implementations;
using WorkingTimeTracker.Core.Validators;
using WorkingTimeTracker.Implementations.Services;
using Xunit;

namespace WorkingTimeTracker.Test.Services
{
    public class UserServiceTest
    {
        private IUserService InstantiateUserService(params Tuple<Type, object>[] arguments)
        {
            var type = typeof(UserService);
            var constructor = type.GetConstructor(new[]
            {
                typeof(IValidationService),
                typeof(IAuthorizationService),
                typeof(IUserRepository),
                typeof(IUserManager),
                typeof(ICurrentUserResolver),
                typeof(IQueryParser)
            });

            var validationService = arguments
                .FirstOrDefault(a => a.Item1 == typeof(IValidationService))
                ?.Item2;
            if (validationService == null)
            {
                validationService = ValidationService.DefaultInstance;
            }

            var authorizationService = arguments
                .FirstOrDefault(a => a.Item1 == typeof(IAuthorizationService))
                ?.Item2;
            if (authorizationService == null)
            {
                authorizationService = new AuthorizationService();
            }

            var userRepository = arguments
                .FirstOrDefault(a => a.Item1 == typeof(IUserRepository))
                ?.Item2;
            if (userRepository == null)
            {
                userRepository = new Mock<IUserRepository>().Object;
            }

            var userManager = arguments
                .FirstOrDefault(a => a.Item1 == typeof(IUserManager))
                ?.Item2;
            if (userManager == null)
            {
                userManager = new Mock<IUserManager>().Object;
            }

            var currentUserResolver = arguments
                .FirstOrDefault(a => a.Item1 == typeof(ICurrentUserResolver))
                ?.Item2;
            if (currentUserResolver == null)
            {
                currentUserResolver = new Mock<ICurrentUserResolver>().Object;
            }

            var queryParser = arguments
                .FirstOrDefault(a => a.Item1 == typeof(IQueryParser))
                ?.Item2;
            if (queryParser == null)
            {
                queryParser = new QueryParser();
            }

            return constructor.Invoke(new[] {
                validationService,
                authorizationService,
                userRepository,
                userManager,
                currentUserResolver,
                queryParser
            }) as IUserService;
        }

        #region Upsert user
        [Fact(DisplayName = "When upsert a new user with valid data, it calls UserManager to create account.")]
        public async Task UpsertUser_ValidData_CreateAccount()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .Returns(Task.FromResult<User>(null));

            userRepositoryMock
                .Setup(r => r.CreateUser(It.IsAny<User>()))
                .Returns(Task.FromResult(0));

            User passedUser = null;
            var userManagerMock = new Mock<IUserManager>();
            userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<User>()))
                .Callback<User>(u => passedUser = u)
                .Returns(Task.FromResult(0));

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object),
                new Tuple<Type, object>(typeof(IUserManager), userManagerMock.Object)
            );
            var command = new UpsertUserCommand()
            {
                Id = "160db952-ce02-44b4-adb9-224d635cc11c",
                Email = "newuser@test.local",
                Name = "New User",
                Roles = new[] { "User", "Admin" }
            };

            // Act
            await userService.UpsertUser(command);

            // Assert
            userManagerMock.Verify(um => um.CreateAsync(It.IsAny<User>()), Times.Once);
            Assert.NotNull(passedUser);
            Assert.Equal(command.Id, passedUser.Id);
            Assert.Equal(command.Email, passedUser.Email);
            Assert.Equal(command.Name, passedUser.Name);
        }

        [Fact(DisplayName = "When upsert a user, it checks if email is unique.")]
        public async Task UpsertUser_ValidData_CheckUniqueEmail()
        {
            // Arrange
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock
                .Setup(r => r.IsEmailUnique(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            var upsertUserCommandValidator = new UpsertUserCommandValidator(userRepoMock.Object);

            var validationService = new ValidationService(upsertUserCommandValidator);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IValidationService), validationService)
            );
            var command = new UpsertUserCommand()
            {
                Id = "160db952-ce02-44b4-adb9-224d635cc11c",
                Email = "newuser@test.local",
                Name = "New User",
                Roles = new[] { "User", "Admin" }
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => userService.UpsertUser(command)
            );
        }

        [Fact(DisplayName = "When upsert a new user with valid data, it calls UserRepository to persist user.")]
        public async Task UpsertUser_ValidData_CallUserRepository()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .Returns(Task.FromResult<User>(null));

            User passedUser = null;
            userRepositoryMock
                .Setup(r => r.CreateUser(It.IsAny<User>()))
                .Callback<User>(u => passedUser = u)
                .Returns(Task.FromResult(0));

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object)
            );
            var command = new UpsertUserCommand()
            {
                Id = "160db952-ce02-44b4-adb9-224d635cc11c",
                Email = "newuser@test.local",
                Name = "New User",
                Roles = new [] { "User", "Admin" }
            };

            // Act
            await userService.UpsertUser(command);

            // Assert
            userRepositoryMock.Verify(r => r.GetUserById(command.Id));
            userRepositoryMock.Verify(r => r.CreateUser(It.IsAny<User>()));
            Assert.NotNull(passedUser);
            Assert.Equal(command.Id, passedUser.Id);
            Assert.Equal(command.Email, passedUser.Email);
            Assert.Equal(command.Name, passedUser.Name);
        }

        [Fact(DisplayName = "When upsert an existing user with valid data, it calls UserRepository to update user.")]
        public async Task UpsertUser_ExistingUser_UpdateUser()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();

            var user = new User()
            {
                Id = id,
                Email = "existing_user@test.local",
                Name = "Existing User",
                PreferredWorkingHourPerDay = 8
            };
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .Returns(Task.FromResult(user));

            User passedUser = null;
            userRepositoryMock
                .Setup(r => r.UpdateUser(It.IsAny<User>()))
                .Callback<User>(u => passedUser = u)
                .Returns(Task.FromResult(0));

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object)
            );
            var command = new UpsertUserCommand()
            {
                Id = id,
                Email = "updated@test.local",
                Name = "Updated User",
                Roles = new[] { "User", "Admin" }
            };

            // Act
            await userService.UpsertUser(command);

            // Assert
            userRepositoryMock.Verify(r => r.GetUserById(command.Id));
            userRepositoryMock.Verify(r => r.UpdateUser(user));

            Assert.NotNull(passedUser);
            Assert.Equal(command.Id, passedUser.Id);
            Assert.Equal(command.Email, passedUser.Email);
            Assert.Equal(command.Name, passedUser.Name);
        }

        [Fact(DisplayName = "When Roles is not null, it calls UserManager to update roles.")]
        public async Task UpsertUser_HasRoles_UpdateRoles()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .Returns(Task.FromResult(new User()
                {
                    Id = id,
                    Email = "existing_user@test.local",
                    Name = "Existing User",
                    PreferredWorkingHourPerDay = 8
                }));

            userRepositoryMock
                .Setup(r => r.CreateUser(It.IsAny<User>()))
                .Returns(Task.FromResult(0));

            var userManagerMock = new Mock<IUserManager>();
            string[] passedRoles = new string[0];
            userManagerMock
                .Setup(r => r.UpdateUserRoles(It.IsAny<string>(), It.IsAny<string[]>()))
                .Callback<string, string[]>((uid, roles) => passedRoles = roles)
                .Returns(Task.FromResult(0));

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserManager), userManagerMock.Object),
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object)
            );
            var command = new UpsertUserCommand()
            {
                Id = id,
                Email = "updated@test.local",
                Name = "Updated User",
                Roles = new[] { "User", "Admin" }
            };

            // Act
            await userService.UpsertUser(command);

            // Assert
            userManagerMock.Verify(um => um.UpdateUserRoles(id, It.IsAny<string[]>()));
            Assert.Equal(command.Roles, passedRoles);
        }

        [Fact(DisplayName = "When Roles is null, it doesn't call UserManager to update roles.")]
        public async Task UpsertUser_NoRoles_DoNotUpdateRoles()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .Returns(Task.FromResult(new User() { Id = id }));

            userRepositoryMock
                .Setup(r => r.CreateUser(It.IsAny<User>()))
                .Returns(Task.FromResult(0));

            var userManagerMock = new Mock<IUserManager>();

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserManager), userManagerMock.Object),
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object)
            );
            var command = new UpsertUserCommand()
            {
                Id = id,
                Email = "updated@test.local",
                Name = "Updated User",
                Roles = null
            };

            // Act
            await userService.UpsertUser(command);

            // Assert
            userManagerMock.Verify(um => um.UpdateUserRoles(id, It.IsAny<string[]>()), Times.Never);
        }

        [Theory(DisplayName = "When upsert a user with invalid data, it throws exception.")]
        [InlineData("", "email@test.local", "Empty Id", new string[] { Constants.ROLE_USER })]
        [InlineData("ff142633-789d-4128-b7fc-f829dcc8f9bf", "", "Empty email", new string[] { Constants.ROLE_USER })]
        [InlineData("5eedbca4-d818-4553-b7a2-0e8b206d71c7", "empty_name@test.local", "", new string[] { Constants.ROLE_USER })]
        [InlineData("c9d3abd4-5f08-45b8-94bf-25aceefdb5c3", "email@test.local", "Empty roles", new string[0] )]
        [InlineData("51fe939a-3ea8-47c4-a6ab-cae5a522a123", "email@test.local", "John", new string[] { "UNSUPPORTED_ROLE" })]
        public async Task UpsertUser_InvalidData_ThrowException(string id, string email, string name, string[] roles)
        {
            // Arrange
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock
                .Setup(r => r.IsEmailUnique(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            var upsertUserCommandValidator = new UpsertUserCommandValidator(userRepoMock.Object);

            var validationService = new ValidationService(upsertUserCommandValidator);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IValidationService), validationService)
            );

            var command = new UpsertUserCommand()
            {
                Id = id,
                Email = email,
                Name = name,
                Roles = roles
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => userService.UpsertUser(command)
            );
        }

        [Fact(DisplayName = "When unauthorized principal upserts a user, it throws exception.")]
        public async Task UpsertUser_Unauthorized_ThrowException()
        {
            // Arrange
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(auth => auth.AuthorizeResource(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<object>()))
                .Throws<AuthorizationException>();

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object)
            );
            var command = new UpsertUserCommand()
            {
                Id = Guid.NewGuid().ToString(),
                Email = "myemail@test.local",
                Name = "Test User",
                Roles = new[] { Constants.ROLE_USER }
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => userService.UpsertUser(command)
            );
        }
        #endregion Upsert user

        #region Delete user

        [Theory(DisplayName = "When delete a user with invalid user id, it throws exception.")]
        [InlineData("")]
        [InlineData(" ")]
        public async Task DeleteUser_InvalidUserId_ThrowException(string userId)
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var userManagerMock = new Mock<IUserManager>();
            var currentUserResolverMock = new Mock<ICurrentUserResolver>();

            var deleteUserCommandValidator = new DeleteUserCommandValidator(
                userManagerMock.Object, 
                userRepositoryMock.Object, 
                currentUserResolverMock.Object);

            var validationService = new ValidationService(deleteUserCommandValidator);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IValidationService), validationService)
            );
            var command = new DeleteUserCommand()
            {
                UserId = userId
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => userService.DeleteUser(command)
            );
        }

        [Theory(DisplayName = "When delete the only admin/user manager, it throws exception.")]
        [InlineData(0, 1)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        public async Task DeleteUser_IsOnlyAdminOrManager_ThrowException(int adminCount, int managerCount)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new User()
                {
                    Id = userId
                });

            var userManagerMock = new Mock<IUserManager>();
            userManagerMock
                .Setup(um => um.GetRoles(userId))
                .ReturnsAsync(new[] { Constants.ROLE_AMIN, Constants.ROLE_USER_MANGER });

            userManagerMock
                .Setup(um => um.CountUsersInRoles())
                .ReturnsAsync(new Dictionary<string, int>()
                {
                    [Constants.ROLE_AMIN] = adminCount,
                    [Constants.ROLE_USER_MANGER] = managerCount,
                    [Constants.ROLE_USER] = 10,
                });

            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync())
                .ReturnsAsync(new User() { Id = Guid.NewGuid().ToString() });

            var deleteUserCommandValidator = new DeleteUserCommandValidator(
                userManagerMock.Object, 
                userRepositoryMock.Object,
                currentUserResolverMock.Object
            );

            var validationService = new ValidationService(deleteUserCommandValidator);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IValidationService), validationService)
            );
            var command = new DeleteUserCommand()
            {
                UserId = userId
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => userService.DeleteUser(command)
            );
        }

        [Fact(DisplayName = "When delete user without force but user has time entries, it throws exception.")]
        public async Task DeleteUser_WithoutForceButHasTimeEntries_ThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new User()
                {
                    Id = userId,
                    TimeEntries = new List<TimeEntry>() { new TimeEntry() }
                });

            var userManagerMock = new Mock<IUserManager>();
            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync())
                .ReturnsAsync(new User() { Id = Guid.NewGuid().ToString() });

            var deleteUserCommandValidator = new DeleteUserCommandValidator(
                userManagerMock.Object, 
                userRepositoryMock.Object,
                currentUserResolverMock.Object
            );

            var validationService = new ValidationService(deleteUserCommandValidator);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IValidationService), validationService)
            );
            var command = new DeleteUserCommand()
            {
                UserId = userId
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => userService.DeleteUser(command)
            );
        }

        [Fact(DisplayName = "When delete a user, it checks if current principal has permission to delete.")]
        public async Task DeleteUser_Unauthorized_ThrowException()
        {
            // Arrange
            const string id = "718aeae3-7ad8-4e3e-8935-705133d95a3c";
            var userToBeDeleted = new User()
            {
                Id = id
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(userToBeDeleted);
            userRepositoryMock.SetReturnsDefault<Task>(Task.FromResult(0));

            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(a => a.AuthorizeResource(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<object>()))
                .Throws<AuthorizationException>();

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object),
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object)
            );
            var command = new DeleteUserCommand()
            {
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => userService.DeleteUser(command)
            );
        }
        
        [Fact(DisplayName = "When delete a user without force enabled, it throws exception if user has time entries.")]
        public async Task DeleteUser_NoForceButHasTimeEntries_ThrowException()
        {
            // Arrange
            const string id = "718aeae3-7ad8-4e3e-8935-705133d95a3c";
            var userToBeDeleted = new User()
            {
                Id = id
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(userToBeDeleted);

            userRepositoryMock
                .Setup(r => r.DeleteUser(It.IsAny<User>(), It.IsAny<bool>()))
                .Throws<RelationshipException>();

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object)
            );
            var command = new DeleteUserCommand()
            {
                UserId = Guid.NewGuid().ToString(),
                ForceDelete = false
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<DeleteUserException>(
                () => userService.DeleteUser(command)
            );
            userRepositoryMock.Verify(r => r.DeleteUser(userToBeDeleted, command.ForceDelete));
        }

        [Fact(DisplayName = "When delete a user with force enabled, it does cascade delete.")]
        public async Task DeleteUser_ForceEnabled_CascadeDelete()
        {
            // Arrange
            const string id = "718aeae3-7ad8-4e3e-8935-705133d95a3c";
            var userToBeDeleted = new User()
            {
                Id = id
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(userToBeDeleted);

            userRepositoryMock
                .Setup(r => r.DeleteUser(It.IsAny<User>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(0));

            var userManagerMock = new Mock<IUserManager>();
            userManagerMock.SetReturnsDefault<Task>(Task.FromResult(0));

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object),
                new Tuple<Type, object>(typeof(IUserManager), userManagerMock.Object)
            );
            var command = new DeleteUserCommand()
            {
                UserId = id,
                ForceDelete = true
            };

            // Act
            await userService.DeleteUser(command);

            // Assert
            userRepositoryMock.Verify(r => r.DeleteUser(userToBeDeleted, command.ForceDelete));
        }

        [Fact(DisplayName = "When delete a user successfully, it deletes his account.")]
        public async Task DeleteUser_OnSuccess_DeleteAccount()
        {
            // Arrange
            const string id = "718aeae3-7ad8-4e3e-8935-705133d95a3c";
            var userToBeDeleted = new User()
            {
                Id = id
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(userToBeDeleted);

            userRepositoryMock
                .Setup(r => r.DeleteUser(It.IsAny<User>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(0));

            var userManagerMock = new Mock<IUserManager>();
            userManagerMock
                .Setup(um => um.RemoveUser(It.IsAny<string>()))
                .Returns(Task.FromResult(0));

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object),
                new Tuple<Type, object>(typeof(IUserManager), userManagerMock.Object)
            );
            var command = new DeleteUserCommand()
            {
                UserId = id,
                ForceDelete = true
            };

            // Act
            await userService.DeleteUser(command);

            // Assert
            userManagerMock.Verify(um => um.RemoveUser(id));
        }

        [Fact(DisplayName = "When delete a non-existing user, it does nothing.")]
        public async Task DeleteUser_NonExisting_DoNothing()
        {
            // Arrange
            const string id = "718aeae3-7ad8-4e3e-8935-705133d95a3c";

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(r => r.GetUserById(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            userRepositoryMock.SetReturnsDefault<Task>(Task.FromResult(0));

            var userManagerMock = new Mock<IUserManager>();
            userManagerMock.SetReturnsDefault<Task>(Task.FromResult(0));

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object),
                new Tuple<Type, object>(typeof(IUserManager), userManagerMock.Object)
            );
            var command = new DeleteUserCommand() { UserId = id };

            // Act
            await userService.DeleteUser(command);

            // Assert
            userRepositoryMock.Verify(um => um.GetUserById(id));
            userRepositoryMock.Verify(um => um.DeleteUser(It.IsAny<User>(), It.IsAny<bool>()), Times.Never);
            userManagerMock.Verify(um => um.RemoveUser(It.IsAny<string>()), Times.Never);
        }

        #endregion Delete user

        #region Get users

        [Fact(DisplayName = "When get users with valid query, result is sorted by Name by default.")]
        public async Task GetUsers_ValidQuery_SortedByNameByDefault()
        {
            // Arrange
            var users = new User[]
            {
                new User() { Id = Guid.NewGuid().ToString(), Name = "B first" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "C first" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "A first" }
            }.AsQueryable();

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUsers())
                .Returns(users);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepoMock.Object)
            );
            var query = new GetUsersQuery();

            // Act
            var result = await userService.GetUsersWithRoles(query);

            // Assert
            var lastMinName = string.Empty;
            Assert.All(result.Results, r =>
            {
                Assert.True(string.CompareOrdinal(r.Name, lastMinName) > 0);
                lastMinName = r.Name;
            });
        }

        [Theory(DisplayName = "When get users with page size greater than max page size, it takes max page size.")]
        [InlineData("?$skip=0&$top=100", 100)]
        [InlineData("?$skip=0&$top=3", 3)]
        public async Task GetUsers_PageSizeGreaterThanMaxPageSize_UseMaxPageSize(string queryString, int top)
        {
            // Arrange
            var originalMaxPageSize = Constants.MaxPageSize;
            Constants.MaxPageSize = 2;

            var users = new User[]
            {
                new User() { Id = Guid.NewGuid().ToString(), Name = "B first" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "C first" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "A first" }
            }.AsQueryable();

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUsers())
                .Returns(users);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepoMock.Object)
            );
            var query = new GetUsersQuery()
            {
                QueryString = queryString,
                Top = top
            };

            // Act
            var result = await userService.GetUsersWithRoles(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Results.Length);

            // Rollback default max page size setting
            Constants.MaxPageSize = originalMaxPageSize;
        }

        [Fact(DisplayName = "When get users with valid query, users must have roles.")]
        public async Task GetUsers_ValidQuery_UsersHaveRoles()
        {
            // Arrange
            var userRoles = new Dictionary<string, string[]>()
            {
                [Guid.NewGuid().ToString()] = new string[] { Constants.ROLE_AMIN, Constants.ROLE_USER },
                [Guid.NewGuid().ToString()] = new string[] { Constants.ROLE_AMIN },
                [Guid.NewGuid().ToString()] = new string[] { Constants.ROLE_USER },
                [Guid.NewGuid().ToString()] = new string[] { Constants.ROLE_USER_MANGER },
                [Guid.NewGuid().ToString()] = new string[] { Constants.ROLE_USER_MANGER, Constants.ROLE_USER }
            };

            var users = userRoles
                .Select(r => new User() { Id = r.Key })
                .AsQueryable();

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUsers())
                .Returns(users);

            var userManagerMock = new Mock<IUserManager>();
            foreach (var userRole in userRoles)
            {
                userManagerMock.Setup(um => um.GetRoles(userRole.Key))
                    .ReturnsAsync(userRole.Value);
            }

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserManager), userManagerMock.Object),
                new Tuple<Type, object>(typeof(IUserRepository), userRepoMock.Object)
            );

            var query = new GetUsersQuery();

            // Act
            var result = await userService.GetUsersWithRoles(query);

            // Assert
            Assert.All(result.Results, r =>
            {
                Assert.NotNull(r.Roles);
                Assert.NotEmpty(r.Roles);
            });
        }

        [Fact(DisplayName = "When unauthorized principal get users, it thows exception.")]
        public async Task GetUsers_Unauthorized_ThrowException()
        {
            // Arrange
            var authorizationServiceMock = new Mock<IAuthorizationService>();

            authorizationServiceMock
                .Setup(a => a.AuthorizeResourceType(It.IsAny<ClaimsPrincipal>(), It.IsAny<Operation>(), It.IsAny<Type>()))
                .Throws<AuthorizationException>();

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IAuthorizationService), authorizationServiceMock.Object)
            );
            var query = new GetUsersQuery();

            // Act
            // Assert
            await Assert.ThrowsAsync<AuthorizationException>(
                () => userService.GetUsersWithRoles(query)
            );
        }

        [Theory(DisplayName = "When get users with paging option in query string, result must be paged.")]
        [InlineData("?$skip=0&$top=1&", 1, 1)]
        [InlineData("?$skip=0&$top=2&", 2, 2)]
        [InlineData("?$skip=0&$top=3&", 3, 3)]
        [InlineData("?$skip=1&$top=2&", 2, 2)]
        [InlineData("?$skip=1&$top=1&", 1, 1)]
        [InlineData("?$skip=2&$top=2&", 3, 1)]
        [InlineData("?$skip=2&$top=3&", 3, 1)]
        [InlineData("?$skip=3&$top=1&", 1, 0)]
        public async Task GetUsers_PagingOption_PagedResult(string queryString, int top, int expectedLength)
        {
            // Arrange
            var originalMaxPageSize = Constants.MaxPageSize;
            Constants.MaxPageSize = 10;

            var users = new User[]
            {
                new User() { Id = Guid.NewGuid().ToString(), Name = "B first" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "C first" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "A first" }
            }.AsQueryable();

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUsers())
                .Returns(users);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepoMock.Object)
            );
            var query = new GetUsersQuery()
            {
                QueryString = queryString,
                Top = top
            };

            // Act
            var result = await userService.GetUsersWithRoles(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLength, result.Results.Length);

            // Rollback max page size setting
            Constants.MaxPageSize = originalMaxPageSize;
        }

        [Theory(DisplayName = "When get users with valid query, it parses query string successfully.")]
        [InlineData("?$filter=Name eq 'Johny Deep'", 1)]
        [InlineData("?$filter=Email ne 'rowling@test.local' and Name eq 'J.K Rowling'", 1)]
        [InlineData("?$filter=Email ne 'deep_pitt@test.local'", 3)]
        [InlineData("?$filter=Name eq 'J.K Rowling'", 2)]
        [InlineData("?$filter=Email eq 'rowling@test.local' or Name eq 'Brad Pitt'", 2)]
        public async Task GetUsers_ValidQuery_ParseQueryString(string queryString, int expectedCount)
        {
            // Arrange
            var users = new User[]
            {
                new User() { Id = Guid.NewGuid().ToString(), Name = "Johny Deep", Email = "deep@test.local" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "Brad Pitt", Email = "deep_pitt@test.local" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "J.K Rowling", Email = "rowling@test.local" },
                new User() { Id = Guid.NewGuid().ToString(), Name = "J.K Rowling", Email = "rowling@test.prod" }
            }.AsQueryable();

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUsers())
                .Returns(users);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(IUserRepository), userRepoMock.Object)
            );
            var query = new GetUsersQuery()
            {
                QueryString = queryString
            };

            // Act
            var result = await userService.GetUsersWithRoles(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Results.Length);
        }


        [Theory(DisplayName = "When get users, IncludeMe must be respected.")]
        [InlineData(true, 1)]
        [InlineData(false, 2)]
        public async Task GetUser_CheckIncludeMe(bool excludeMe, int expectedCount)
        {
            // Arrange
            var me = new User() { Id = Guid.NewGuid().ToString() };
            var anotherUser = new User() { Id = Guid.NewGuid().ToString() };

            var users = new[] { me, anotherUser };

            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync())
                .ReturnsAsync(me);

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUsers())
                .Returns(users.AsQueryable());

            var query = new GetUsersQuery()
            {
                ExcludeMe = excludeMe
            };

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object),
                new Tuple<Type, object>(typeof(IUserRepository), userRepoMock.Object)
            );

            // Act
            var result = await userService.GetUsersWithRoles(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Results.Length);
        }

        #endregion Get users

        #region Get current user

        [Fact(DisplayName = "When get current user, it get user from repository.")]
        public async Task GetCurrentUser_GetUserFromRepo()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync())
                .ReturnsAsync(new User() { Id = userId });

            var expectedRoles = new[] { Constants.ROLE_USER, Constants.ROLE_USER_MANGER };
            var userManagerMock = new Mock<IUserManager>();
            userManagerMock.Setup(um => um.GetRoles(It.IsAny<string>()))
                .ReturnsAsync(expectedRoles);

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object),
                new Tuple<Type, object>(typeof(IUserManager), userManagerMock.Object)
            );

            // Act
            var result = await userService.GetCurrentUserWithRoles();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(expectedRoles, result.Roles);

            currentUserResolverMock.Verify(r => r.ResolveAsync());
            userManagerMock.Verify(um => um.GetRoles(userId));
        }

        #endregion Get current user

        #region Update user setting

        [Theory(DisplayName = "When update user setting with invalid input, it throws exception")]
        [InlineData("", 8)]
        [InlineData(" ", 8)]
        [InlineData("Valid name", -1)]
        public async Task UpdateUserSetting_InvalidInput_ThrowException(string name, int preferredWorkingHoursPerDay)
        {
            // Arrange
            var command = new UpdateCurrentUserSettingsCommand()
            {
                Name = name,
                PreferredWorkingHourPerDay = preferredWorkingHoursPerDay
            };
            var userService = InstantiateUserService();

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => userService.UpdateCurrentUserSettings(command)
            );
        }

        [Fact(DisplayName = "When update user setting, it saves in persistence.")]
        public async Task UpdateUserSetting_SaveInPersistence()
        {
            // Arrange
            var command = new UpdateCurrentUserSettingsCommand()
            {
                Name = "Updated name",
                PreferredWorkingHourPerDay = 10
            };

            var currentUser = new User() { Id = Guid.NewGuid().ToString() };
            var currentUserResolverMock = new Mock<ICurrentUserResolver>();
            currentUserResolverMock.Setup(r => r.ResolveAsync())
                .ReturnsAsync(currentUser);

            var userRepoMock = new Mock<IUserRepository>();
            User passedUser = null;
            userRepoMock.Setup(r => r.UpdateUser(It.IsAny<User>()))
                .Callback<User>(u => passedUser = u)
                .Returns(Task.FromResult(0));

            var userService = InstantiateUserService(
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolverMock.Object),
                new Tuple<Type, object>(typeof(IUserRepository), userRepoMock.Object)
            );

            // Act
            await userService.UpdateCurrentUserSettings(command);

            // Assert
            userRepoMock.Verify(u => u.UpdateUser(passedUser));
            Assert.Equal(command.Name, passedUser.Name);
            Assert.Equal(command.PreferredWorkingHourPerDay, passedUser.PreferredWorkingHourPerDay);
        }

        #endregion Update user setting
    }
}
