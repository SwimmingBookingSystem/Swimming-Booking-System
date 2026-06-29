using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.TicketTypes.Commands.SeedDefaultTickets;

// ── Command 
public record SeedDefaultTicketsCommand : IRequest<SuccessResponse>;

// ── Handler 
public class SeedDefaultTicketsCommandHandler
    : IRequestHandler<SeedDefaultTicketsCommand, SuccessResponse>
{
    private readonly IUnitOfWork _uow;

    public SeedDefaultTicketsCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(SeedDefaultTicketsCommand _, CancellationToken ct)
    {
        // 3 loại vé đơn mặc định
        var defaults = new[]
        {
            new { Code = "ADULT",  Name = "Vé người lớn",             Base = 100_000m, Disc = 0m  },
            new { Code = "CHILD",  Name = "Vé trẻ em dưới 12 tuổi",  Base = 100_000m, Disc = 30m },
            new { Code = "SENIOR", Name = "Vé người cao tuổi",        Base = 100_000m, Disc = 20m },
        };

        // Lấy tất cả pool đang Active để gán vé
        var pools = await _uow.ToListAsync(
            _uow.Repository<Pool>().Query().Where(p => p.Status == "Active"), ct);

        int seeded = 0;

        foreach (var d in defaults)
        {
            // Idempotent: nếu đã tồn tại (theo Code hoặc Name) thì bỏ qua
            bool exists = await _uow.AnyAsync(
                _uow.Repository<TicketType>().Query()
                    .Where(t => t.TicketCode == d.Code || t.TicketName == d.Name), ct);
            if (exists) continue;

            // Tạo loại vé
            var ticket = new TicketType
            {
                TicketCode      = d.Code,
                TicketName      = d.Name,
                Category        = "Single",
                BasePrice       = d.Base,
                DiscountPercent = d.Disc,
                Status          = "Active",
                CreatedAt       = DateTime.UtcNow
            };

            await _uow.Repository<TicketType>().AddAsync(ticket, ct);
            await _uow.SaveChangesAsync(ct); // flush để lấy TicketTypeId

            // Giá thực = BasePrice * (1 - Discount/100)
            decimal actualPrice = d.Base * (1 - d.Disc / 100m);

            // Gán vào tất cả pool Active
            foreach (var pool in pools)
            {
                await _uow.Repository<PoolTicketType>().AddAsync(new PoolTicketType
                {
                    PoolId       = pool.PoolId,
                    TicketTypeId = ticket.TicketTypeId,
                    Price        = actualPrice,
                    Status       = "Active"
                }, ct);
            }

            if (pools.Count > 0)
                await _uow.SaveChangesAsync(ct);

            seeded++;
        }

        string msg = seeded == 0
            ? "Tất cả vé đơn mặc định đã tồn tại, không cần seed lại."
            : $"Đã seed thành công {seeded} loại vé đơn mặc định.";

        return new SuccessResponse { Message = msg };
    }
}
