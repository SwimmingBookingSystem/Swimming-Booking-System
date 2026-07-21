using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Services.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Services.Implementations;

public class TicketManagementService : ITicketManagementService
{
    private readonly IUnitOfWork _uow;

    public TicketManagementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task ClosePoolTicketAsync(int poolId, int ticketTypeId, CancellationToken ct)
    {
        var pt = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolTicketType>().Query()
                .Where(x => x.PoolId == poolId && x.TicketTypeId == ticketTypeId), ct)
            ?? throw new NotFoundException("PoolTicketType", $"Pool {poolId} – TicketType {ticketTypeId}");

        if (pt.Status == "Inactive")
            throw new BadRequestException("Vé tại bể bơi này đã ở trạng thái Inactive.");

        // Kiểm tra xem vé này có đang được sử dụng trong Booking nào chưa hoàn thành không
        bool hasActiveBookings = await _uow.AnyAsync(
            _uow.Repository<BookingDetail>().Query()
                .Where(bd => bd.PoolTicketTypeId == pt.PoolTicketTypeId 
                          && (bd.Booking.Status == "PendingPayment" || bd.Booking.Status == "Confirmed")), ct);

        if (hasActiveBookings)
        {
            throw new BadRequestException("Không thể ngừng áp dụng vé vì đang có khách hàng (Booking) chờ thanh toán hoặc đã xác nhận sử dụng vé này tại bể bơi.");
        }

        pt.Status = "Inactive";
        _uow.Repository<PoolTicketType>().Update(pt);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task OpenPoolTicketAsync(int poolId, int ticketTypeId, CancellationToken ct)
    {
        var ticketType = await _uow.FirstOrDefaultAsync(
            _uow.Repository<TicketType>().Query().Where(t => t.TicketTypeId == ticketTypeId), ct);

        if (ticketType != null && ticketType.Status == "Inactive")
        {
            throw new BadRequestException("Loại vé gốc đang ngừng kinh doanh. Vui lòng mở lại Loại vé gốc trước khi mở bán tại bể bơi này.");
        }

        var pt = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolTicketType>().Query()
                .Where(x => x.PoolId == poolId && x.TicketTypeId == ticketTypeId), ct);

        if (pt == null)
        {
            pt = new PoolTicketType
            {
                PoolId = poolId,
                TicketTypeId = ticketTypeId,
                Price = null,
                Status = "Active"
            };
            await _uow.Repository<PoolTicketType>().AddAsync(pt);
            await _uow.SaveChangesAsync(ct);
            return;
        }

        if (pt.Status == "Active")
            throw new BadRequestException("Vé tại bể bơi này đã ở trạng thái Active.");

        pt.Status = "Active";
        _uow.Repository<PoolTicketType>().Update(pt);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task UpdatePoolTicketPriceAsync(int poolId, int ticketTypeId, decimal? customPrice, CancellationToken ct)
    {
        var pt = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolTicketType>().Query()
                .Where(x => x.PoolId == poolId && x.TicketTypeId == ticketTypeId), ct)
            ?? throw new NotFoundException("PoolTicketType", $"Pool {poolId} – TicketType {ticketTypeId}");

        if (pt.Price != customPrice)
        {
            await _uow.Repository<PoolTicketPriceHistory>().AddAsync(new PoolTicketPriceHistory
            {
                PoolTicketTypeId = pt.PoolTicketTypeId,
                OldCustomPrice = pt.Price,
                NewCustomPrice = customPrice,
                ModifiedAt = System.DateTime.UtcNow,
                ModifiedByUserName = "Manager" // Lấy từ HttpContext nếu có
            }, ct);
        }

        pt.Price = customPrice;
        _uow.Repository<PoolTicketType>().Update(pt);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task CloseTicketTypeAsync(int ticketTypeId, CancellationToken ct)
    {
        var ticket = await _uow.FirstOrDefaultAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketTypeId == ticketTypeId), ct)
            ?? throw new NotFoundException(nameof(TicketType), ticketTypeId);

        if (ticket.Status == "Inactive")
            throw new BadRequestException("Loại vé đã ở trạng thái Inactive, không cần đóng lại.");

       

        ticket.Status = "Inactive";
        _uow.Repository<TicketType>().Update(ticket);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task OpenTicketTypeAsync(int ticketTypeId, CancellationToken ct)
    {
        var ticket = await _uow.FirstOrDefaultAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketTypeId == ticketTypeId), ct)
            ?? throw new NotFoundException(nameof(TicketType), ticketTypeId);

        if (ticket.Status == "Active")
            throw new BadRequestException("Loại vé đã ở trạng thái Active.");

        ticket.Status = "Active";
        _uow.Repository<TicketType>().Update(ticket);
        await _uow.SaveChangesAsync(ct);
    }
}
