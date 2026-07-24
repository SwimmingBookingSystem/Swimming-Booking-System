using MediatR;
using SBS.Application.Common;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.CancelWaitlist;

public class CancelWaitlistCommandHandler : IRequestHandler<CancelWaitlistCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelWaitlistCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(CancelWaitlistCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var currentUserId))
        {
            throw new UnauthorizedAccessException("Bạn chưa đăng nhập.");
        }

        var waitlistRepo = _unitOfWork.Repository<WaitlistEntry>();
        var entry = await waitlistRepo.GetByIdAsync(request.WaitlistEntryId);

        if (entry == null)
        {
            throw new InvalidOperationException("Không tìm thấy lượt hàng chờ cần hủy.");
        }

        if (entry.UserId != currentUserId)
        {
            throw new System.UnauthorizedAccessException("Bạn không có quyền hủy đăng ký hàng chờ này.");
        }

        if (entry.Status != WaitlistStatus.Waiting)
        {
            throw new InvalidOperationException($"Không thể hủy lượt hàng chờ vì trạng thái hiện tại là '{WaitlistStatus.ToDisplayName(entry.Status)}'.");
        }

        entry.Status = WaitlistStatus.Cancelled;
        waitlistRepo.Update(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
