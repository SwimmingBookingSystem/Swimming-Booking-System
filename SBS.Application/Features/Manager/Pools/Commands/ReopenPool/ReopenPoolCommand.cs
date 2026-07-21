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
    private readonly SBS.Application.Features.Manager.Services.Interfaces.IPoolManagementService _poolService;

    public ReopenPoolCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.IPoolManagementService poolService) => _poolService = poolService;

    public async Task<SuccessResponse> Handle(ReopenPoolCommand request, CancellationToken ct)
    {
        return await _poolService.ReopenPoolAsync(request.PoolId, ct);
    }
}

