using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.TicketTypes.Dtos;
using SBS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.PoolTickets.Queries.GetTicketsByPool;

public record GetTicketsByPoolQuery(int PoolId) : IRequest<List<PoolTicketTypeDto>>;

public class GetTicketsByPoolQueryHandler
    : IRequestHandler<GetTicketsByPoolQuery, List<PoolTicketTypeDto>>
{
    private readonly IUnitOfWork _uow;

    public GetTicketsByPoolQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<PoolTicketTypeDto>> Handle(
        GetTicketsByPoolQuery request, CancellationToken ct)
    {
        return await _uow.ToListAsync(
            _uow.Repository<PoolTicketType>().Query()
                .Where(pt => pt.PoolId == request.PoolId)
                .OrderBy(pt => pt.TicketType.Category)
                .ThenBy(pt => pt.TicketType.TicketCode)
                .Select(pt => new PoolTicketTypeDto
                {
                    PoolTicketTypeId = pt.PoolTicketTypeId,
                    PoolId           = pt.PoolId,
                    PoolName         = pt.Pool.PoolName,
                    TicketTypeId     = pt.TicketTypeId,
                    TicketCode       = pt.TicketType.TicketCode,
                    TicketName       = pt.TicketType.TicketName,
                    Category         = pt.TicketType.Category,
                    BasePrice        = pt.TicketType.BasePrice,
                    DiscountPercent  = pt.TicketType.DiscountPercent,
                    Price            = pt.Price,
                    Status           = pt.Status
                }), ct);
    }
}
