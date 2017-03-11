using System;
using System.Security.Claims;

namespace WorkingTimeTracker.Core.Authorization
{
    public interface IResourceAuthorizationRequirement
    {
        bool CanValidate(Operation operation, object resource);

        RequirementValidationResult Validate(ClaimsPrincipal user, Operation operation, object resource);
    }

    public interface IResourceTypeAuthorizationRequirement
    {
        bool CanValidate(Operation operation, Type type);

        RequirementValidationResult Validate(ClaimsPrincipal user, Operation operation, Type type);
    }
}
