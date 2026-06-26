using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Commands.ClosePool;

//  Command 
public record ClosePoolCommand(int PoolId) : IRequest<SuccessResponse>;

//  Handler 
public class ClosePoolCommandHandler : IRequestHandler<ClosePoolCommand, SuccessResponse>
{
    private readonly IUnitOfWork _uow;

    public ClosePoolCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(ClosePoolCommand request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        if (pool.Status == "Closed")
            throw new BadRequestException("Bể bơi đã ở trạng thái Closed, không thể đóng lại.");

        pool.Status    = "Closed";
        pool.UpdatedAt = DateTime.UtcNow;

        _uow.Repository<Pool>().Update(pool);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã tạm đóng bể bơi thành công." };
    }
}
