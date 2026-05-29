using FluentValidation;
using SBS.Application.Features.CheckIn.DTOs;

namespace SBS.Application.Features.CheckIn.Validators;

public class VerifyTicketRequestValidator : AbstractValidator<VerifyTicketRequestDto>
{
    public VerifyTicketRequestValidator()
    {
        RuleFor(x => x.TicketCode)
            .NotEmpty().WithMessage("Mã vé không được để trống.")
            .MaximumLength(50).WithMessage("Mã vé không được vượt quá 50 ký tự.");
    }
}
