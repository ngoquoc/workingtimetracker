using System.Collections.Generic;
using System.Threading.Tasks;
using WorkingTimeTracker.Core.Validators;

namespace WorkingTimeTracker.Core.Services
{
    public interface IValidationService
    {
        Task Validate<T>(T obj);
    }

    public class ValidationService : IValidationService
    {
        protected IValidator[] validators;

        public static ValidationService DefaultInstance
        {
            get
            {
                var service = new ValidationService();
                service.validators = new IValidator[]
                {
                    new RegisterCommandValidator(),
                    new ChangePasswordCommandValidator(),
                    new DeleteTimeEntryCommandValidator(),
                    new UpdateCurrentUserSettingsCommandValidator()
                };
                return service;
            }
        }

        public ValidationService(params IValidator[] validators)
        {
            this.validators = validators;
        }

        async Task IValidationService.Validate<T>(T obj)
        {
            foreach (var validator in validators)
            {
                if (validator.CanValidate(obj))
                {
                    await validator.Validate(obj);
                }
            }
        }
    }
}
