using FluentValidation;

namespace SBS.Application.Features.Contacts.Commands.CreateContactRequest;

public class CreateContactRequestCommandValidator : AbstractValidator<CreateContactRequestCommand>
{
    public CreateContactRequestCommandValidator()
    {
        RuleFor(v => v.FullName)
            .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.")
            .NotEmpty().WithMessage("FullName is required.");

        RuleFor(v => v.Email)
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid Email is required.");

        RuleFor(v => v.Category)
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters.")
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(v => v.Message)
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.")
            .NotEmpty().WithMessage("Message is required.");
    }
}
