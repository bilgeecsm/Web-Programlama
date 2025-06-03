using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace AIRecipeApp.Api.Authorization
{
    public class CustomRoleHandler : AuthorizationHandler<CustomRoleRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CustomRoleRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "role"))
                return Task.CompletedTask;

            var role = context.User.FindFirst(c => c.Type == "role")?.Value;
            if (role == requirement.Role)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}