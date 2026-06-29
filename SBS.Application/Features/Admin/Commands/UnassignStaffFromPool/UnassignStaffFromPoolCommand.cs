using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.UnassignStaffFromPool;

public record UnassignStaffFromPoolCommand : IRequest<ResultDto>
{
    public int PoolId { get; init; }

    public Guid StaffId { get; init; }
}

public class UnassignStaffFromPoolCommandHandler : IRequestHandler<UnassignStaffFromPoolCommand, ResultDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnassignStaffFromPoolCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultDto> Handle(UnassignStaffFromPoolCommand request, CancellationToken cancellationToken)
    {
        // 1. Tìm bản ghi phân công
        var assignment = await _unitOfWork.FirstOrDefaultAsync(
            _unitOfWork.Repository<PoolStaffAssignment>().Query()
                .Where(a => a.PoolId == request.PoolId && a.StaffId == request.StaffId),
            cancellationToken);

        if (assignment is null)
            return ResultDto.Failure(new[] { "Không tìm thấy phân công này." });

        // 2. Xoá bản ghi
        _unitOfWork.Repository<PoolStaffAssignment>().Delete(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ResultDto.Success();
    }
}
