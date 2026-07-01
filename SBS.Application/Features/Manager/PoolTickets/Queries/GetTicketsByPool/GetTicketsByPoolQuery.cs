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
        // Lấy toàn bộ Loại vé đang Active trên hệ thống
        var activeTickets = await _uow.ToListAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.Status == "Active")
                .OrderBy(t => t.CreatedAt), ct);

        // Lấy danh sách ánh xạ giá của riêng Bể bơi này
        var poolTickets = await _uow.ToListAsync(
            _uow.Repository<PoolTicketType>().Query()
                .Where(pt => pt.PoolId == request.PoolId), ct);

        var poolName = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query()
                .Where(p => p.PoolId == request.PoolId)
                .Select(p => p.PoolName), ct);

        var result = new List<PoolTicketTypeDto>();

        foreach (var ticket in activeTickets)
        {
            var pt = poolTickets.FirstOrDefault(p => p.TicketTypeId == ticket.TicketTypeId);

            result.Add(new PoolTicketTypeDto
            {
                PoolTicketTypeId = pt?.PoolTicketTypeId ?? 0,
                PoolId           = request.PoolId,
                PoolName         = poolName ?? "Bể bơi chưa xác định",
                TicketTypeId     = ticket.TicketTypeId,
                TicketCode       = ticket.TicketCode,
                TicketName       = ticket.TicketName,
                Category         = ticket.Category,
                BasePrice        = ticket.BasePrice,
                DiscountPercent  = ticket.DiscountPercent,
                Price            = pt?.Price, // Nếu null => Rơi tự do về BasePrice
                Status           = pt?.Status ?? "NotApplied" // Nếu chưa có record => "Chưa áp dụng"
            });
        }

        return result;
    }
}
