using FluentValidation;

namespace SBS.Application.Features.Admin.Commands.RespondContactRequest;

public class AdminRespondContactRequestCommandValidator : AbstractValidator<AdminRespondContactRequestCommand>
{
    public AdminRespondContactRequestCommandValidator()
    {
        RuleFor(x => x.ContactRequestId)
            .GreaterThan(0).WithMessage("ContactRequestId không hợp lệ.");

        RuleFor(x => x.ResponseMessage)
            .NotEmpty().WithMessage("Nội dung phản hồi không được để trống.")
            .MaximumLength(2000).WithMessage("Nội dung phản hồi không được vượt quá 2000 ký tự.");
    }
}
