using FluentValidation;

namespace Identity.Application.Commands.RemoveRole
{
    /// <summary>
    /// Validator for RemoveRoleCommand
    /// </summary>
    public class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
    {
        public RemoveRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters");
        }
    }
}
