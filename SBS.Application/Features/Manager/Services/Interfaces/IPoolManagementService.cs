using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Features.Manager.Pools.Commands.CreatePool;
using SBS.Application.Features.Manager.Pools.Commands.UpdatePool;
using SBS.Application.Features.Manager.Pools.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Services.Interfaces;

public interface IPoolManagementService
{
    Task<SuccessResponse> ClosePoolAsync(int poolId, CancellationToken ct);
    Task<CreatePoolResponse> CreatePoolAsync(CreatePoolCommand request, CancellationToken ct);
    Task<SuccessResponse> ReopenPoolAsync(int poolId, CancellationToken ct);
    Task<PoolDto> UpdatePoolAsync(UpdatePoolCommand request, CancellationToken ct);
}
