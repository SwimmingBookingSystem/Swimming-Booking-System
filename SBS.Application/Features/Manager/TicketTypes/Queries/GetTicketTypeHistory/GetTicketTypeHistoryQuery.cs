using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.TicketTypes.Queries.GetTicketTypeHistory;

public record GetTicketTypeHistoryQuery(int TicketTypeId) : IRequest<List<TicketPriceHistoryDto>>;

public class TicketPriceHistoryDto
{
    public int Id { get; set; }
    public decimal OldBasePrice { get; set; }
    public decimal NewBasePrice { get; set; }
    public decimal OldDiscountPercent { get; set; }
    public decimal NewDiscountPercent { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? ModifiedByUserName { get; set; }
}

public class GetTicketTypeHistoryQueryHandler : IRequestHandler<GetTicketTypeHistoryQuery, List<TicketPriceHistoryDto>>
{
    private readonly IReadOnlyUnitOfWork _uow;
    public GetTicketTypeHistoryQueryHandler(IReadOnlyUnitOfWork uow) => _uow = uow;

    public async Task<List<TicketPriceHistoryDto>> Handle(GetTicketTypeHistoryQuery request, CancellationToken ct)
    {
        var ticket = await _uow.FirstOrDefaultAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketTypeId == request.TicketTypeId), ct)
            ?? throw new NotFoundException(nameof(TicketType), request.TicketTypeId);

        var history = await _uow.ToListAsync(
            _uow.Repository<TicketPriceHistory>().Query()
                .Where(h => h.TicketTypeId == request.TicketTypeId)
                .OrderBy(h => h.ModifiedAt), ct);

        return history.Select(h => new TicketPriceHistoryDto
        {
            Id = h.Id,
            OldBasePrice = h.OldBasePrice,
            NewBasePrice = h.NewBasePrice,
            OldDiscountPercent = h.OldDiscountPercent,
            NewDiscountPercent = h.NewDiscountPercent,
            ModifiedAt = h.ModifiedAt,
            ModifiedByUserName = h.ModifiedByUserName
        }).ToList();
    }
}
