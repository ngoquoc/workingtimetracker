using System;
using System.Threading.Tasks;

namespace WorkingTimeTracker.Core.Validators
{
    public interface IValidator
    {
        bool CanValidate(object obj);

        Task Validate(object obj);
    }

    public abstract class Validator<T> : IValidator
        where T : class
    {
        bool IValidator.CanValidate(object obj)
        {
            return obj is T;
        }

        Task IValidator.Validate(object obj)
        {
            var stronglyTypedObj = obj as T;
            return Validate(stronglyTypedObj);
        }

        public abstract Task Validate(T obj);
    }
}
