using FluentValidation;

namespace Identity.Application.Commands.AssignRole
{
    /// <summary>
    /// Validator for AssignRoleCommand
    /// </summary>
    public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
    {
        public AssignRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters");
        }
    }
}
