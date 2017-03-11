using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WorkingTimeTracker.Core.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        protected IResourceAuthorizationRequirement[] resourceRequirements = new IResourceAuthorizationRequirement[]
            {
            };

        protected IResourceTypeAuthorizationRequirement[] resourceTypeRequirements = new IResourceTypeAuthorizationRequirement[]
            {

            };

        public void SetResourceRequirements(params IResourceAuthorizationRequirement[] resourceRequirements)
        {
            this.resourceRequirements = resourceRequirements;
        }

        public void SetResourceTypeRequirements(params IResourceTypeAuthorizationRequirement[] resourceTypeRequirements)
        {
            this.resourceTypeRequirements = resourceTypeRequirements;
        }

        Task IAuthorizationService.AuthorizeResource(ClaimsPrincipal user, Operation operation, object resource)
        {
            bool skipped = false;
            foreach (var requirement in resourceRequirements)
            {
                if (!requirement.CanValidate(operation, resource))
                {
                    continue;
                }

                var validationResult = requirement.Validate(user, operation, resource);

                if (validationResult.Status == RequirementValidationStatus.Fail)
                {
                    throw new AuthorizationException(string.Join("\n", validationResult.Errors));
                }
                else if (validationResult.Status == RequirementValidationStatus.Succeed)
                {
                    return Task.FromResult(0);
                }

                skipped = true;
            }

            if (skipped)
            {
                throw new AuthorizationException();
            }
            else
            {
                return Task.FromResult(0);
            }
        }

        Task IAuthorizationService.AuthorizeResourceType(ClaimsPrincipal user, Operation operation, Type type)
        {
            bool skipped = false;

            foreach (var requirement in resourceTypeRequirements)
            {
                if (!requirement.CanValidate(operation, type))
                {
                    continue;
                }

                var validationResult = requirement.Validate(user, operation, type);

                if (validationResult.Status == RequirementValidationStatus.Fail)
                {
                    throw new AuthorizationException(string.Join("\n", validationResult.Errors));
                }
                else if (validationResult.Status == RequirementValidationStatus.Succeed)
                {
                    return Task.FromResult(0);
                }

                skipped = true;
            }

            if (skipped)
            {
                throw new AuthorizationException();
            }
            else
            {
                return Task.FromResult(0);
            }
        }
    }
}
