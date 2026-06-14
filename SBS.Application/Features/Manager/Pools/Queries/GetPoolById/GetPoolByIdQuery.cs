using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Queries.GetPoolById;

public record GetPoolByIdQuery(int PoolId) : IRequest<PoolDto>;

public class GetPoolByIdQueryHandler : IRequestHandler<GetPoolByIdQuery, PoolDto>
{
    private readonly IUnitOfWork _uow;

    public GetPoolByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PoolDto> Handle(GetPoolByIdQuery request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query()
                .Include(p => p.PoolImages)
                .Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        return new PoolDto
        {
            PoolId      = pool.PoolId,
            PoolName    = pool.PoolName,
            Address     = pool.Address,
            Description = pool.Description,
            ImageUrl    = pool.ImageUrl,
            Images      = pool.PoolImages.Select(img => new PoolImageDto 
            {
                PoolImageId = img.PoolImageId,
                ImageUrl    = img.ImageUrl,
                IsCover     = img.IsCover,
                SortOrder   = img.SortOrder,
                CreatedAt   = img.CreatedAt
            }).OrderBy(i => i.SortOrder).ToList(),
            OpeningTime = pool.OpeningTime.ToString(@"hh\:mm"),
            ClosingTime = pool.ClosingTime.ToString(@"hh\:mm"),
            Status      = pool.Status,
            CreatedAt   = pool.CreatedAt,
            UpdatedAt   = pool.UpdatedAt
        };
    }
}
