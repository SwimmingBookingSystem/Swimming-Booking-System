using FluentValidation;
using System;

namespace SBS.Application.Features.Admin.Commands.AssignStaffToPool;

public class AssignStaffToPoolCommandValidator : AbstractValidator<AssignStaffToPoolCommand>
{
    public AssignStaffToPoolCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .GreaterThan(0).WithMessage("Vui lòng chọn hồ bơi hợp lệ.");

        RuleFor(x => x.StaffId)
            .NotEqual(Guid.Empty).WithMessage("Vui lòng chọn nhân viên hợp lệ.");
    }
}
