using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.ServiceStaff.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.ServiceStaff.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetPoolServiceDetailQuery(int PoolServiceId) : IRequest<PoolServiceDto>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetPoolServiceDetailQueryHandler : IRequestHandler<GetPoolServiceDetailQuery, PoolServiceDto>
{
    private readonly IApplicationDbContext _context;

    public GetPoolServiceDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PoolServiceDto> Handle(GetPoolServiceDetailQuery request, CancellationToken cancellationToken)
    {
        var service = await _context.PoolServices
            .Include(ps => ps.Pool)
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.PoolServiceId == request.PoolServiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy dịch vụ bể bơi với ID {request.PoolServiceId}.");

        return new PoolServiceDto
        {
            PoolServiceId = service.PoolServiceId,
            PoolId = service.PoolId,
            PoolName = service.Pool.PoolName,
            ServiceName = service.ServiceName,
            Description = service.Description,
            Price = service.Price,
            ServiceImage = service.ServiceImage,
            Quantity = service.Quantity,
            ServiceStatus = service.ServiceStatus
        };
    }
}
