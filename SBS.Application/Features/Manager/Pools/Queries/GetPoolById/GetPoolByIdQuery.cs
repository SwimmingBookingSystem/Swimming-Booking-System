using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Queries.GetPoolById;

public record GetPoolByIdQuery(int PoolId) : IRequest<PoolDto>;

public class GetPoolByIdQueryHandler : IRequestHandler<GetPoolByIdQuery, PoolDto>
{
    private readonly IReadOnlyUnitOfWork _uow;

    public GetPoolByIdQueryHandler(IReadOnlyUnitOfWork uow) => _uow = uow;

    public async Task<PoolDto> Handle(GetPoolByIdQuery request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query()
                .Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        var poolImages = await _uow.ToListAsync(
            _uow.Repository<PoolImage>().Query()
                .Where(img => img.PoolId == pool.PoolId)
                .OrderBy(img => img.SortOrder), ct);

        return new PoolDto
        {
            PoolId      = pool.PoolId,
            PoolName    = pool.PoolName,
            Address     = pool.Address,
            Description = pool.Description,
            Images      = poolImages.Select(img => new PoolImageDto 
            {
                PoolImageId = img.PoolImageId,
                ImageUrl    = img.ImageUrl,
                IsCover     = img.IsCover,
                SortOrder   = img.SortOrder,
                CreatedAt   = img.CreatedAt
            }).ToList(),
            OpeningTime = pool.OpeningTime.ToString(@"hh\:mm"),
            ClosingTime = pool.ClosingTime.ToString(@"hh\:mm"),
            Status      = pool.Status,
            Area        = pool.Area,
            StandardCapacity = pool.StandardCapacity,
            CreatedAt   = pool.CreatedAt,
            UpdatedAt   = pool.UpdatedAt
        };
    }
}
