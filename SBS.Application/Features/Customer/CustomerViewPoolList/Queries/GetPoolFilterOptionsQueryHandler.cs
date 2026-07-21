using MediatR;
using SBS.Application.Common.Dtos.Customer.CustomerViewPoolList;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer.CustomerViewPoolList.Queries;

public class GetPoolFilterOptionsQueryHandler : IRequestHandler<GetPoolFilterOptionsQuery, PoolFilterOptionsDto>
{
    private readonly IReadOnlyUnitOfWork _uow;

    public GetPoolFilterOptionsQueryHandler(IReadOnlyUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PoolFilterOptionsDto> Handle(GetPoolFilterOptionsQuery request, CancellationToken ct)
    {
        var poolsQuery = _uow.Repository<Pool>().Query().Where(p => p.Status == "Active");

        var openingTimesQuery = poolsQuery
            .Select(p => new TimeOptionDto { Time = p.OpeningTime })
            .Distinct();

        var closingTimesQuery = poolsQuery
            .Select(p => new TimeOptionDto { Time = p.ClosingTime })
            .Distinct();

        var openingTimesDtos = await _uow.ToListAsync(openingTimesQuery, ct);
        var closingTimesDtos = await _uow.ToListAsync(closingTimesQuery, ct);

        var openingTimes = openingTimesDtos
            .Select(x => x.Time)
            .OrderBy(t => t)
            .Select(t => t.ToString(@"hh\:mm\:ss"))
            .ToList();

        var closingTimes = closingTimesDtos
            .Select(x => x.Time)
            .OrderBy(t => t)
            .Select(t => t.ToString(@"hh\:mm\:ss"))
            .ToList();

        return new PoolFilterOptionsDto
        {
            OpeningTimes = openingTimes,
            ClosingTimes = closingTimes
        };
    }
}
