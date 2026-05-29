using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.ServiceStaff.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.ServiceStaff.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetPoolServicesQuery(int PoolId, string? Status) : IRequest<IReadOnlyList<PoolServiceDto>>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetPoolServicesQueryHandler : IRequestHandler<GetPoolServicesQuery, IReadOnlyList<PoolServiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPoolServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PoolServiceDto>> Handle(GetPoolServicesQuery request, CancellationToken cancellationToken)
    {
        // Check if Pool exists
        var poolExists = await _context.Pools.AnyAsync(p => p.PoolId == request.PoolId, cancellationToken);
        if (!poolExists)
            throw new KeyNotFoundException($"Không tìm thấy bể bơi với ID {request.PoolId}.");

        var query = _context.PoolServices
            .Where(ps => ps.PoolId == request.PoolId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statusLower = request.Status.ToLower();
            query = query.Where(ps => ps.ServiceStatus.ToLower() == statusLower);
        }

        var services = await query
            .Select(ps => new PoolServiceDto
            {
                PoolServiceId = ps.PoolServiceId,
                PoolId = ps.PoolId,
                ServiceName = ps.ServiceName,
                Description = ps.Description,
                Price = ps.Price,
                ServiceImage = ps.ServiceImage,
                Quantity = ps.Quantity,
                ServiceStatus = ps.ServiceStatus
            })
            .OrderBy(ps => ps.ServiceName)
            .ToListAsync(cancellationToken);

        return services;
    }
}
