using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Moq;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;
using WorkingTimeTracker.Core.Repositories;
using WorkingTimeTracker.Core.Services;
using WorkingTimeTracker.Core.Validators;
using WorkingTimeTracker.Implementations.Services;
using Xunit;

namespace WorkingTimeTracker.Test.Services
{
    public class AuthenticationServiceTest
    {
        private IAuthenticationService InstantiateAuthenticationService(params Tuple<Type, object>[] arguments)
        {
            var type = typeof(AspNetIdentityAuthenticationService);
            var constructor = type.GetConstructor(new[]
            {
                typeof(IValidationService),
                typeof(IAspNetIdentityUserManager),
                typeof(ISignInManager),
                typeof(IUserRepository),
                typeof(ICurrentUserResolver)
            });

            var validationService = arguments
                .FirstOrDefault(a => a.Item1 == typeof(IValidationService))
                ?.Item2;
            if (validationService == null)
            {
                validationService = ValidationService.DefaultInstance;
            }

            var userRepository = arguments
                .FirstOrDefault(a => a.Item1 == typeof(IUserRepository))
                ?.Item2;
            if (userRepository == null)
            {
                userRepository = new Mock<IUserRepository>().Object;
            }

            var userManager = arguments
                .FirstOrDefault(a => a.Item1 == typeof(IAspNetIdentityUserManager))
                ?.Item2;
            if (userManager == null)
            {
                var userManagerMock = new Mock<IAspNetIdentityUserManager>();
                userManagerMock.SetReturnsDefault<Task>(Task.FromResult(0));
                userManager = userManagerMock.Object;
            }

            var signInManager = arguments
                .FirstOrDefault(a => a.Item1 == typeof(ISignInManager))
                ?.Item2;
            if (signInManager == null)
            {
                signInManager = new Mock<ISignInManager>().Object;
            }

            var currentUserResolver = arguments
                .FirstOrDefault(a => a.Item1 == typeof(ICurrentUserResolver))
                ?.Item2;
            if (currentUserResolver == null)
            {
                currentUserResolver = new Mock<ICurrentUserResolver>().Object;
            }

            return constructor.Invoke(new[] {
                validationService,
                userManager,
                signInManager,
                userRepository,
                currentUserResolver
            }) as IAuthenticationService;
        }

        #region Register

        [Fact(DisplayName = "When user registers with correct info, it makes a call to user manager to register.")]
        public async Task Register_CorrectInfo_CallUserManager()
        {
            // Arrange
            var userManagerMock = new Mock<IAspNetIdentityUserManager>();
            IdentityUser passedIdentityUser = null;
            string passedPassword = string.Empty;
            userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .Callback<IdentityUser, string>((identityUser, password) =>
                {
                    passedIdentityUser = identityUser;
                    passedPassword = password;
                })
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock
                .Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser());

            var userServiceMock = new Mock<IUserService>();
            userServiceMock
                .Setup(us => us.UpsertUser(It.IsAny<UpsertUserCommand>()))
                .ReturnsAsync(It.IsAny<User>());

            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(IAspNetIdentityUserManager), userManagerMock.Object),
                new Tuple<Type, object>(typeof(IUserService), userServiceMock.Object)
            );

            var command = new RegisterCommand()
            {
                Email = "john@toptal.local",
                Name = "John Doe",
                Password = "mypassword123",
                PasswordConfirm = "mypassword123"
            };

            // Act
            await authenticationService.Register(command);

            // Assert
            userManagerMock.Verify(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()));
            Assert.Equal(command.Email, passedIdentityUser.Email);
            Assert.Equal(command.Password, passedPassword);
        }

        [Theory(DisplayName = "When user registers with wrong info, it throws ValidationException.")]
        [InlineData("", "Empty email", "mypassword123", "mypassword123")]
        [InlineData("john@toptal.local", "", "mypassword123", "mypassword123")]
        [InlineData("wrongpwconfim@toptal.local", "Wrong PW confirm", "mypassword123", "otherpassword")]
        [InlineData("john@toptal.local", "Empty PW", "", "")]
        [InlineData("john.toptal.local", "Wrong email format", "mypassword123", "mypassword123")]
        [InlineData("john@toptal.local", "Too short password", "pw123", "pw123")]
        [InlineData("john@toptal.local", "Insecured password", "password", "password")]
        public async Task Register_WrongInfo_ThrowException(
            string email, string name, string password, string passwordConfirm)
        {
            // Arrange
            var authenticationService = InstantiateAuthenticationService();

            var command = new RegisterCommand()
            {
                Email = email,
                Name = name,
                Password = password,
                PasswordConfirm = passwordConfirm
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                authenticationService.Register(command)
            );
        }

        [Fact(DisplayName = "When user manager fails to register user, it throws exception.")]
        public async Task Register_UserManagerFails_ThrowException()
        {
            // Arrange
            var userManagerMock = new Mock<IAspNetIdentityUserManager>();
            userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());
            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(IAspNetIdentityUserManager), userManagerMock.Object)
            );

            var command = new RegisterCommand()
            {
                Email = "john@toptal.local",
                Name = "John Doe",
                Password = "mypassword123",
                PasswordConfirm = "mypassword123"
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<RegistrationException>(() =>
                authenticationService.Register(command)
            );
        }

        [Fact(DisplayName = "When user registers an account, it creates a record of User for him.")]
        public async Task Register_Success_CreateUserRecord()
        {
            // Arrange
            const string email = "john@toptal.local";
            string userId = Guid.NewGuid().ToString();

            var userManagerMock = new Mock<IAspNetIdentityUserManager>();
            userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock
                .Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser()
                {
                    Id = userId,
                    Email = email
                });

            var userRepositoryMock = new Mock<IUserRepository>();
            User passedUser = null;
            userRepositoryMock
                .Setup(us => us.CreateUser(It.IsAny<User>()))
                .Callback<User>(u => passedUser = u)
                .Returns(Task.FromResult(0));

            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(IAspNetIdentityUserManager), userManagerMock.Object),
                new Tuple<Type, object>(typeof(IUserRepository), userRepositoryMock.Object)
            );

            var command = new RegisterCommand()
            {
                Email = email,
                Name = "John Doe",
                Password = "mypassword123",
                PasswordConfirm = "mypassword123"
            };

            // Act
            await authenticationService.Register(command);

            // Assert
            userRepositoryMock.Verify(r => r.CreateUser(It.IsAny<User>()));
            Assert.Equal(userId, passedUser.Id);
            Assert.Equal(command.Email, passedUser.Email);
            Assert.Equal(command.Name, passedUser.Name);
        }

        [Fact(DisplayName = "When user registers an account, it adds him to User role.")]
        public async Task Register_Success_AddToUserRole()
        {
            // Arrange
            const string userId = "93e8513f-c689-4780-9c40-598d580b8dc4";

            var userManagerMock = new Mock<IAspNetIdentityUserManager>();
            userManagerMock
                .Setup(um => um.UpdateUserRoles(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(Task.FromResult(0));

            userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            userManagerMock
                .Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser() { Id = userId });

            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(IAspNetIdentityUserManager), userManagerMock.Object)
            );

            var command = new RegisterCommand()
            {
                Email = "john@toptal.local",
                Name = "John Doe",
                Password = "mypassword123",
                PasswordConfirm = "mypassword123"
            };

            // Act
            await authenticationService.Register(command);

            // Assert
            userManagerMock.Verify(um => um.UpdateUserRoles(
                userId,
                It.Is<string[]>(roles => roles.Length == 1 && roles[0] == Core.Constants.ROLE_USER)
            ));
        }

        #endregion Register

        #region Login

        [Fact(DisplayName = "When user logs in with valid credential, it makes a call to SignInManager with correct data.")]
        public async Task Login_ValidCredential_CallSiginManager()
        {
            // Arrange
            const string userName = "john@toptal.local";
            const string password = "mypassword1234";

            var siginManagerMock = new Mock<ISignInManager>();
            siginManagerMock.Setup(sm =>
                sm.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInStatus.Success);

            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(ISignInManager), siginManagerMock.Object)
            );

            // Act
            await authenticationService.Login(userName, password);

            // Assert
            siginManagerMock.Verify(sm => sm.PasswordSignInAsync(userName, password, true, false));
        }

        [Theory(DisplayName = "When user logs in with invalid credential, it throws exception.")]
        [InlineData(SignInStatus.Failure)]
        [InlineData(SignInStatus.LockedOut)]
        public async Task Login_InvalidCredential_ThrowException(SignInStatus signInStatus)
        {
            // Arrange
            const string userName = "john@toptal.local";
            const string password = "bad_password";

            var siginManagerMock = new Mock<ISignInManager>();
            siginManagerMock.Setup(sm =>
                sm.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(signInStatus);

            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(ISignInManager), siginManagerMock.Object)
            );

            // Act
            // Assert
            await Assert.ThrowsAsync<LoginException>(
                () => authenticationService.Login(userName, password)
            );
        }

        #endregion Login

        #region ChangePassword

        [Fact(DisplayName = "When user changes password with correct info, it makes a call to UserManager with correct data.")]
        public async Task ChangePassword_CorrectInfo_CallUserManager()
        {
            // Arrange
            const string userId = "160db952-ce02-44b4-adb9-224d635cc11c";

            var currentUserResolver = new Mock<ICurrentUserResolver>();
            currentUserResolver.Setup(r => r.ResolveAsync())
                .ReturnsAsync(new User()
                {
                    Id = userId
                });

            var userManagerMock = new Mock<IAspNetIdentityUserManager>();
            userManagerMock
                .Setup(um => um.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolver.Object),
                new Tuple<Type, object>(typeof(IAspNetIdentityUserManager), userManagerMock.Object)
            );

            var command = new ChangePasswordCommand()
            {
                OldPassword = "old_password_123",
                NewPassword = "mypassword123",
                NewPasswordConfirm = "mypassword123"
            };

            // Act
            await authenticationService.ChangePassword(command);

            // Assert
            userManagerMock.Verify(um => um.ChangePasswordAsync(userId, command.OldPassword, command.NewPassword));
        }

        [Theory(DisplayName = "When user changes password with invalid input, it throws exception.")]
        [InlineData("", "empty_old_pass", "empty_old_pass")]
        [InlineData("empty_new_pass", "", "")]
        [InlineData("oldpassword123", "short", "short")]
        [InlineData("oldpassword123", "insecuredpw", "insecuredpw")]
        [InlineData("oldpassword123", "newpassword123", "unmatchedpassword123")]
        [InlineData("unchangedPassword1", "unchangedPassword1", "unchangedPassword1")]
        public async Task ChangePassword_InvalidInput_ThrowException(string oldPassword, string newPassword, string newPasswordConfirm)
        {
            // Arrange
            var authenticationService = InstantiateAuthenticationService();

            var command = new ChangePasswordCommand()
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                NewPasswordConfirm = newPasswordConfirm
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => authenticationService.ChangePassword(command)
            );
        }

        [Fact(DisplayName = "When anonymous user changes password, it thows exception.")]
        public async Task ChangePassword_Anonymous_ThrowException()
        {
            // Arrange
            var currentUserResolver = new Mock<ICurrentUserResolver>();
            currentUserResolver
                .Setup(r => r.ResolveAsync())
                .ReturnsAsync((User)null);

            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolver.Object)
            );

            var command = new ChangePasswordCommand()
            {
                OldPassword = "old_password_123",
                NewPassword = "mypassword123",
                NewPasswordConfirm = "mypassword123"
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ChangePasswordException>(
                () => authenticationService.ChangePassword(command)
            );
        }

        [Fact(DisplayName = "When it fails to change password, it throws exception.")]
        public async Task ChangePassword_Fail_ThrowException()
        {
            // Arrange
            var currentUserResolver = new Mock<ICurrentUserResolver>();
            currentUserResolver.Setup(r => r.ResolveAsync())
                .ReturnsAsync(new User());

            var userManagerMock = new Mock<IAspNetIdentityUserManager>();
            userManagerMock
                .Setup(um => um.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            var authenticationService = InstantiateAuthenticationService(
                new Tuple<Type, object>(typeof(ICurrentUserResolver), currentUserResolver.Object),
                new Tuple<Type, object>(typeof(IAspNetIdentityUserManager), userManagerMock.Object)
            );

            var command = new ChangePasswordCommand()
            {
                OldPassword = "old_password_123",
                NewPassword = "mypassword123",
                NewPasswordConfirm = "mypassword123"
            };

            // Act
            // Assert
            await Assert.ThrowsAsync<ChangePasswordException>(
                () => authenticationService.ChangePassword(command)
            );
        }

        #endregion ChangePassword
    }
}
