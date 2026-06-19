using FluentValidation;
using System;

namespace SBS.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
            .MinimumLength(3).WithMessage("Tên đăng nhập phải có ít nhất 3 ký tự.")
            .MaximumLength(50).WithMessage("Tên đăng nhập không quá 50 ký tự.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Tên đăng nhập chỉ được chứa chữ cái, chữ số và dấu gạch dưới.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống.")
            .Equal(x => x.Password).WithMessage("Xác nhận mật khẩu không khớp.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ và tên không được để trống.")
            .MaximumLength(100).WithMessage("Họ và tên không quá 100 ký tự.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[0-9]{10,11}$").WithMessage("Số điện thoại không hợp lệ, phải gồm 10-11 số.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Dob)
            .Must(dob => dob == null || dob < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Ngày sinh không hợp lệ.");
    }
}
