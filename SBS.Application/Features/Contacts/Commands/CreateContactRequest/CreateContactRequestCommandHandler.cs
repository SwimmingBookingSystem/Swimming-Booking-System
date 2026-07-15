using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;

namespace SBS.Application.Features.Contacts.Commands.CreateContactRequest;

public class CreateContactRequestCommandHandler : IRequestHandler<CreateContactRequestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public CreateContactRequestCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<bool> Handle(CreateContactRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.UserId;
        Guid? userId = null;
        if (!string.IsNullOrEmpty(currentUserIdString) && Guid.TryParse(currentUserIdString, out var parsedGuid))
        {
            userId = parsedGuid;
        }

        var contactRequest = new ContactRequest
        {
            UserId = userId,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Category = request.Category,
            Message = request.Message,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var repo = _unitOfWork.Repository<ContactRequest>();
        await repo.AddAsync(contactRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send automated confirmation email
        try
        {
            var emailSubject = $"Hệ thống đã ghi nhận yêu cầu của bạn: {request.Category}";
            var emailBody = $@"
                    <p>Chào {request.FullName},</p>
                    <p>Hệ thống Swimming Booking System (SBS) đã ghi nhận yêu cầu hỗ trợ của bạn với nội dung sau:</p>
                    <ul>
                        <li><strong>Phân loại:</strong> {request.Category}</li>
                        <li><strong>Nội dung:</strong> {request.Message}</li>
                    </ul>
                    <p>Ban quản lý sẽ xem xét và liên hệ lại với bạn qua số điện thoại <b>{request.PhoneNumber ?? "chưa cung cấp"}</b> hoặc email <b>{request.Email}</b> trong thời gian sớm nhất.</p>
                    <p>Trân trọng,<br>Ban quản lý SBS</p>
                    ";
            await _emailService.SendEmailAsync(request.Email, emailSubject, emailBody);
        }
        catch
        {
            // Do not block the request if email sending fails.
        }

        return true;
    }
}
