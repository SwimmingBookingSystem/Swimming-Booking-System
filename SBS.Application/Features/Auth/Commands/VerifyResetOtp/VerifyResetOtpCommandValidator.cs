using FluentValidation;

namespace SBS.Application.Features.Auth.Commands.VerifyResetOtp;

public class VerifyResetOtpCommandValidator : AbstractValidator<VerifyResetOtpCommand>
{
    public VerifyResetOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("Mã OTP không được để trống.")
            .Length(6).WithMessage("Mã OTP phải có 6 ký tự.");
    }
}
