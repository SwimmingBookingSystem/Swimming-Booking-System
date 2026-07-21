using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Queries.GetPools;

public record GetPoolsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    string? Search = null
) : IRequest<PagedResponse<PoolDto>>;

public class GetPoolsQueryHandler : IRequestHandler<GetPoolsQuery, PagedResponse<PoolDto>>
{
    private readonly IReadOnlyUnitOfWork _uow;

    public GetPoolsQueryHandler(IReadOnlyUnitOfWork uow) => _uow = uow;

    public async Task<PagedResponse<PoolDto>> Handle(GetPoolsQuery request, CancellationToken ct)
    {
        var query = _uow.Repository<Pool>().Query();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(p => p.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.PoolName.Contains(request.Search) || p.Address.Contains(request.Search));

        var total = await _uow.CountAsync(query, ct);

        var items = await _uow.ToListAsync(
            query.OrderBy(p => p.CreatedAt)
                 .Skip((request.Page - 1) * request.PageSize)
                 .Take(request.PageSize)
                 .Select(p => new PoolDto
                 {
                     PoolId      = p.PoolId,
                     PoolName    = p.PoolName,
                     Address     = p.Address,
                     Description = p.Description,
                     Images      = p.PoolImages.Select(img => new PoolImageDto 
                     {
                         PoolImageId = img.PoolImageId,
                         ImageUrl    = img.ImageUrl,
                         IsCover     = img.IsCover,
                         SortOrder   = img.SortOrder,
                         CreatedAt   = img.CreatedAt
                     }).OrderBy(i => i.SortOrder).ToList(),
                     OpeningTime = p.OpeningTime.ToString(@"hh\:mm"),
                     ClosingTime = p.ClosingTime.ToString(@"hh\:mm"),
                     Status      = p.Status,
                     Area        = p.Area,
                     StandardCapacity = p.StandardCapacity,
                     CreatedAt   = p.CreatedAt,
                     UpdatedAt   = p.UpdatedAt
                 }), ct);

        return new PagedResponse<PoolDto>
        {
            Items      = items,
            TotalCount = total,
            Page       = request.Page,
            PageSize   = request.PageSize
        };
    }
}
