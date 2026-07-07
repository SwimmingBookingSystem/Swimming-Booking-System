using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.RespondContactRequest;

public record AdminRespondContactRequestCommand : IRequest<ResultDto>
{
    public int ContactRequestId { get; init; }
    public string ResponseMessage { get; init; } = null!;
}

public class AdminRespondContactRequestCommandHandler : IRequestHandler<AdminRespondContactRequestCommand, ResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public AdminRespondContactRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<ResultDto> Handle(AdminRespondContactRequestCommand request, CancellationToken cancellationToken)
    {
        var adminIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out var adminId))
            return ResultDto.Failure(new[] { "Admin chưa đăng nhập hoặc không hợp lệ." });

        var contact = await _unitOfWork.FirstOrDefaultAsync(
            _unitOfWork.Repository<ContactRequest>()
                .Query()
                .Where(c => c.ContactRequestId == request.ContactRequestId),
            cancellationToken);

        if (contact is null)
            return ResultDto.Failure(new[] { "Không tìm thấy yêu cầu hỗ trợ." });

        if (contact.Status != "Pending")
            return ResultDto.Failure(new[] { $"Yêu cầu hỗ trợ này đã được xử lý (Trạng thái: {contact.Status})." });

        contact.Status = "Resolved";
        contact.HandledByUserId = adminId;
        contact.HandledAt = DateTime.UtcNow;

        _unitOfWork.Repository<ContactRequest>().Update(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var emailSubject = $"Phản hồi yêu cầu hỗ trợ: {contact.Category}";
            var emailBody = $@"
                    <p>Chào {contact.FullName},</p>
                    <p>Ban quản lý SBS đã phản hồi yêu cầu hỗ trợ của bạn như sau:</p>
                    <hr/>
                    <p>{request.ResponseMessage}</p>
                    <hr/>
                    <p>Nếu bạn còn thắc mắc, vui lòng liên hệ lại với chúng tôi qua email này.</p>
                    <p>Trân trọng,<br>Ban quản lý SBS</p>";

            await _emailService.SendEmailAsync(contact.Email, emailSubject, emailBody);
        }
        catch
        {
            // Do not block if email sending fails
        }

        return ResultDto.Success();
    }
}
