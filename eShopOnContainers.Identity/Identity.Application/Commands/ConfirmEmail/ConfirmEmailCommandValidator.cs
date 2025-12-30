using FluentValidation;

namespace Identity.Application.Commands.ConfirmEmail
{
    /// <summary>
    /// Validator for ConfirmEmailCommand
    /// </summary>
    public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.ConfirmationToken)
                .NotEmpty().WithMessage("Confirmation token is required");
        }
    }
}
