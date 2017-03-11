using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Commands;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Core.Services
{
    public interface IAuthenticationService
    {
        Task Login(string userName, string password);

        Task Register(RegisterCommand command);

        Task ChangePassword(ChangePasswordCommand command);
    }

    public class RegistrationException : Exception
    {
        public RegistrationException(string message) : base(message) { }

        public RegistrationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class LoginException : Exception
    {
        public LoginException(string message) : base(message) { }

        public LoginException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ChangePasswordException : Exception
    {
        public ChangePasswordException(string message) : base(message) { }

        public ChangePasswordException(string message, Exception innerException) : base(message, innerException) { }
    }
}
