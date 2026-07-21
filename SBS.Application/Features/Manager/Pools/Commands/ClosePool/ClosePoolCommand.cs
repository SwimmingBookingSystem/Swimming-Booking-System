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
    private readonly SBS.Application.Features.Manager.Services.Interfaces.IPoolManagementService _poolService;

    public ClosePoolCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.IPoolManagementService poolService) => _poolService = poolService;

    public async Task<SuccessResponse> Handle(ClosePoolCommand request, CancellationToken ct)
    {
        return await _poolService.ClosePoolAsync(request.PoolId, ct);
    }
}

