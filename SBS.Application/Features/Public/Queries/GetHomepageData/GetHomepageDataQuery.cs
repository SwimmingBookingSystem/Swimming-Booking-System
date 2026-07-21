using MediatR;
using SBS.Application.Common.Dtos.Public;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Public.Queries.GetHomepageData;

public record GetHomepageDataQuery : IRequest<HomepageDataDto>;

public class GetHomepageDataQueryHandler : IRequestHandler<GetHomepageDataQuery, HomepageDataDto>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;

    public GetHomepageDataQueryHandler(IReadOnlyUnitOfWork readOnlyUnitOfWork)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
    }

    public async Task<HomepageDataDto> Handle(GetHomepageDataQuery request, CancellationToken cancellationToken)
    {
        var totalPools = await _readOnlyUnitOfWork.CountAsync(
            _readOnlyUnitOfWork.Repository<Pool>().Query().Where(p => p.Status == "Active"),
            cancellationToken);

        var totalBookings = await _readOnlyUnitOfWork.CountAsync(
            _readOnlyUnitOfWork.Repository<Booking>().Query(),
            cancellationToken);

        var bookingUserIds = await _readOnlyUnitOfWork.ToListAsync(
            _readOnlyUnitOfWork.Repository<Booking>().Query().Select(b => new { b.UserId }),
            cancellationToken);
        var totalCustomers = bookingUserIds.Select(b => b.UserId).Distinct().Count();

        var ratings = await _readOnlyUnitOfWork.ToListAsync(
            _readOnlyUnitOfWork.Repository<Feedback>().Query().Select(f => new { f.Rating }),
            cancellationToken);
        var avgRating = ratings.Count > 0 ? ratings.Average(r => r.Rating) : 0;

        var pools = await _readOnlyUnitOfWork.ToListAsync(
            _readOnlyUnitOfWork.Repository<Pool>()
                .Query()
                .Where(p => p.Status == "Active")
                .OrderBy(p => p.PoolName)
                .Take(6)
                .Select(p => new HomepagePoolDto
                {
                    PoolId = p.PoolId,
                    PoolName = p.PoolName,
                    Address = p.Address,
                    Description = p.Description,
                    CoverImageUrl = p.PoolImages
                        .Where(img => img.IsCover)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                        ?? p.PoolImages.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).FirstOrDefault(),
                    OpeningTime = p.OpeningTime.ToString(@"hh\:mm"),
                    ClosingTime = p.ClosingTime.ToString(@"hh\:mm"),
                    MinPrice = p.PoolTicketTypes
                        .Where(pt => pt.Status == "Active")
                        .Min(pt => (decimal?)pt.Price)
                }),
            cancellationToken);

        return new HomepageDataDto
        {
            Stats = new HomepageStatsDto
            {
                TotalPools = totalPools,
                TotalCustomers = totalCustomers,
                TotalBookings = totalBookings,
                AverageRating = Math.Round(avgRating, 1)
            },
            Pools = pools
        };
    }
}
