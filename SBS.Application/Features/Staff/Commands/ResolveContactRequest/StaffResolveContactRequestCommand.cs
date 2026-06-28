using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Commands.ResolveContactRequest;

public record StaffResolveContactRequestCommand : IRequest<ResultDto>
{
    public int ContactRequestId { get; init; }
}

public class StaffResolveContactRequestCommandHandler : IRequestHandler<StaffResolveContactRequestCommand, ResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public StaffResolveContactRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ResultDto> Handle(StaffResolveContactRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy StaffId
        var staffIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(staffIdString) || !Guid.TryParse(staffIdString, out var staffId))
            return ResultDto.Failure(new[] { "Nhân viên chưa đăng nhập hoặc không hợp lệ." });

        // 2. Tìm contact request
        var contact = await _unitOfWork.FirstOrDefaultAsync(
            _unitOfWork.Repository<ContactRequest>()
                .Query()
                .Where(c => c.ContactRequestId == request.ContactRequestId),
            cancellationToken);

        if (contact is null)
            return ResultDto.Failure(new[] { "Không tìm thấy yêu cầu hỗ trợ." });

        // 3. Validate chỉ resolve được Pending
        if (contact.Status != "Pending")
            return ResultDto.Failure(new[] { $"Yêu cầu hỗ trợ này đã được xử lý (Trạng thái: {contact.Status})." });

        // 4. Cập nhật trạng thái
        contact.Status = "Resolved";
        contact.HandledByUserId = staffId;
        contact.HandledAt = DateTime.UtcNow;

        _unitOfWork.Repository<ContactRequest>().Update(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ResultDto.Success();
    }
}
