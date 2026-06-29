using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.AssignStaffToPool;

public record AssignStaffToPoolCommand : IRequest<ResultDto>
{
    public int PoolId { get; init; }

    public Guid StaffId { get; init; }
}

public class AssignStaffToPoolCommandHandler : IRequestHandler<AssignStaffToPoolCommand, ResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AssignStaffToPoolCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ResultDto> Handle(AssignStaffToPoolCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy AdminId đang đăng nhập
        Guid? adminId = Guid.TryParse(_currentUserService.UserId, out var parsed) ? parsed : null;

        // 2. Kiểm tra Pool tồn tại
        var pool = await _unitOfWork.FirstOrDefaultAsync(
            _unitOfWork.Repository<Pool>().Query()
                .Where(p => p.PoolId == request.PoolId),
            cancellationToken);

        if (pool is null)
            return ResultDto.Failure(new[] { "Hồ bơi không tồn tại." });

        // 3. Kiểm tra chưa được phân công
        var alreadyAssigned = await _unitOfWork.AnyAsync(
            _unitOfWork.Repository<PoolStaffAssignment>().Query()
                .Where(a => a.PoolId == request.PoolId && a.StaffId == request.StaffId),
            cancellationToken);

        if (alreadyAssigned)
            return ResultDto.Failure(new[] { "Staff đã được phân công vào hồ bơi này." });

        // 4. Tạo bản ghi phân công
        var assignment = new PoolStaffAssignment
        {
            PoolId = request.PoolId,
            StaffId = request.StaffId,
            AssignedAt = DateTime.UtcNow,
            AssignedByAdminId = adminId
        };

        await _unitOfWork.Repository<PoolStaffAssignment>().AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ResultDto.Success();
    }
}
