using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Commands.ReopenPool;

//  Command 
public record ReopenPoolCommand(int PoolId) : IRequest<SuccessResponse>;

// Handler
public class ReopenPoolCommandHandler : IRequestHandler<ReopenPoolCommand, SuccessResponse>
{
    private readonly IUnitOfWork _uow;

    public ReopenPoolCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(ReopenPoolCommand request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        if (pool.Status == "Active")
            throw new BadRequestException("Bể bơi đang ở trạng thái Active, không cần mở lại.");

        pool.Status    = "Active";
        pool.UpdatedAt = DateTime.UtcNow;

        _uow.Repository<Pool>().Update(pool);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã mở lại bể bơi thành công." };
    }
}
