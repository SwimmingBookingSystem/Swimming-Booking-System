using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Features.Manager.TicketTypes.Dtos;
using SBS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.TicketTypes.Queries.GetTicketTypeById;

// ── Query 
public record GetTicketTypeByIdQuery(int TicketTypeId) : IRequest<TicketTypeDto>;

// ── Handler 
public class GetTicketTypeByIdQueryHandler
    : IRequestHandler<GetTicketTypeByIdQuery, TicketTypeDto>
{
    private readonly IUnitOfWork _uow;

    public GetTicketTypeByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TicketTypeDto> Handle(
        GetTicketTypeByIdQuery request, CancellationToken ct)
    {
        var ticket = await _uow.FirstOrDefaultAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketTypeId == request.TicketTypeId), ct)
            ?? throw new NotFoundException(nameof(TicketType), request.TicketTypeId);

        // Lấy ComboDetails nếu là Combo
        List<ComboDetailDto>? comboDetails = null;
        if (ticket.Category == "Combo")
        {
            comboDetails = await _uow.ToListAsync(
                _uow.Repository<ComboDetail>().Query()
                    .Where(cd => cd.ComboTicketTypeId == ticket.TicketTypeId)
                    .Select(cd => new ComboDetailDto
                    {
                        ComboDetailId      = cd.ComboDetailId,
                        SingleTicketTypeId = cd.SingleTicketTypeId,
                        SingleTicketCode   = cd.SingleTicketType.TicketCode,
                        SingleTicketName   = cd.SingleTicketType.TicketName,
                        Quantity           = cd.Quantity
                    }), ct);
        }

        return new TicketTypeDto
        {
            TicketTypeId    = ticket.TicketTypeId,
            TicketCode      = ticket.TicketCode,
            TicketName      = ticket.TicketName,
            Category        = ticket.Category,
            BasePrice       = ticket.BasePrice,
            DiscountPercent = ticket.DiscountPercent,
            Description     = ticket.Description,
            Status          = ticket.Status,
            CreatedAt       = ticket.CreatedAt,
            ComboDetails    = comboDetails
        };
    }
}
