using FluentValidation;

namespace SBS.Application.Features.Staff.Commands.ResolveContactRequest;

public class StaffResolveContactRequestCommandValidator : AbstractValidator<StaffResolveContactRequestCommand>
{
    public StaffResolveContactRequestCommandValidator()
    {
        RuleFor(x => x.ContactRequestId)
            .GreaterThan(0).WithMessage("ContactRequestId không hợp lệ.");
    }
}
