using FluentValidation;

namespace SBS.Application.Features.Admin.Commands.ChangeUserRole;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được để trống.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Vai trò không được để trống.")
            .Must(r => r is "Customer" or "Staff" or "Manager" or "Admin")
            .WithMessage("Vai trò không hợp lệ. Các vai trò: Customer, Staff, Manager, Admin.");
    }
}
