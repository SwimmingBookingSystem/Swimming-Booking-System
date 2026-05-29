using FluentValidation;
using SBS.Application.Features.ServiceStaff.DTOs;

namespace SBS.Application.Features.ServiceStaff.Validators;

public class CreateServiceReportRequestValidator : AbstractValidator<CreateServiceReportRequestDto>
{
    public CreateServiceReportRequestValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("ServiceId không hợp lệ.");

        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("StaffId không hợp lệ.");

        RuleFor(x => x.ReportReason)
            .NotEmpty().WithMessage("Lý do báo cáo không được để trống.")
            .MaximumLength(500).WithMessage("Lý do báo cáo tối đa là 500 ký tự.");

        RuleFor(x => x.Suggestion)
            .MaximumLength(500).WithMessage("Ý kiến đề xuất tối đa là 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Suggestion));
    }
}
