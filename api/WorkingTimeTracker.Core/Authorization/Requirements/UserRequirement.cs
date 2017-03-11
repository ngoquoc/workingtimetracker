using System;
using System.Security.Claims;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Core.Authorization.Requirements
{
    public class UserRequirement : IResourceAuthorizationRequirement
    {
        bool IResourceAuthorizationRequirement.CanValidate(Operation operation, object resource)
        {
            return resource is User;
        }

        RequirementValidationResult IResourceAuthorizationRequirement.Validate(ClaimsPrincipal user, Operation operation, object resource)
        {
            if (user.IsInRole(Constants.ROLE_AMIN))
            {
                return RequirementValidationResult.Succeed;
            }

            if (operation != Operation.ForceDelete && user.IsInRole(Constants.ROLE_USER_MANGER))
            {
                return RequirementValidationResult.Succeed;
            }

            return RequirementValidationResult.Failed();
        }
    }

    public class UserTypeRequirement : IResourceTypeAuthorizationRequirement
    {
        bool IResourceTypeAuthorizationRequirement.CanValidate(Operation operation, Type type)
        {
            return type == typeof(User);
        }

        RequirementValidationResult IResourceTypeAuthorizationRequirement.Validate(ClaimsPrincipal user, Operation operation, Type type)
        {
            if (user.IsInRole(Constants.ROLE_AMIN))
            {
                return RequirementValidationResult.Succeed;
            }

            if (operation != Operation.ForceDelete && user.IsInRole(Constants.ROLE_USER_MANGER))
            {
                return RequirementValidationResult.Succeed;
            }

            return RequirementValidationResult.Failed();
        }
    }
}
