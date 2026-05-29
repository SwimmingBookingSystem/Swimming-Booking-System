using FluentValidation;
using System;

namespace SBS.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ và tên không được để trống.")
            .MaximumLength(100).WithMessage("Họ và tên không được vượt quá 100 ký tự.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10,11}$").WithMessage("Số điện thoại không hợp lệ (phải gồm 10 đến 11 chữ số).")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Dob)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Ngày sinh phải là một ngày trong quá khứ.")
            .When(x => x.Dob.HasValue);

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Địa chỉ không được vượt quá 200 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.Address));
    }
}
