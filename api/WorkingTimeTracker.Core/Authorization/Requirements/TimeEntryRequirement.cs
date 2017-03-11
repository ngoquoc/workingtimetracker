using System;
using System.Linq;
using System.Security.Claims;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Core.Authorization.Requirements
{
    public class TimeEntryRequirement : IResourceAuthorizationRequirement
    {
        bool IResourceAuthorizationRequirement.CanValidate(Operation operation, object resource)
        {
            return resource is TimeEntry;
        }

        RequirementValidationResult IResourceAuthorizationRequirement.Validate(ClaimsPrincipal user, Operation operation, object resource)
        {
            if (user.IsInRole(Constants.ROLE_AMIN))
            {
                return RequirementValidationResult.Succeed;
            }

            if (!user.IsInRole(Constants.ROLE_USER))
            {
                return RequirementValidationResult.Failed();
            }

            var timeEntry = resource as TimeEntry;

            var userId = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (timeEntry.OwnerId == userId)
            {
                return RequirementValidationResult.Succeed;
            }

            return RequirementValidationResult.Skip;
        }
    }

    public class TimeEntryTypeRequirement : IResourceTypeAuthorizationRequirement
    {
        bool IResourceTypeAuthorizationRequirement.CanValidate(Operation operation, Type type)
        {
            return type == typeof(TimeEntry);
        }

        RequirementValidationResult IResourceTypeAuthorizationRequirement.Validate(ClaimsPrincipal user, Operation operation, Type type)
        {
            if (type != typeof(TimeEntry))
            {
                return RequirementValidationResult.Skip;
            }

            if (user.IsInRole(Constants.ROLE_AMIN))
            {
                return RequirementValidationResult.Succeed;
            }

            if (!user.IsInRole(Constants.ROLE_USER))
            {
                return RequirementValidationResult.Failed();
            }

            if (operation == Operation.ReadAll)
            {
                return RequirementValidationResult.Failed();
            }
            
            return RequirementValidationResult.Succeed;
        }
    }
}
