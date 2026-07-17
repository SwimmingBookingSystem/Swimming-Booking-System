using FluentValidation;
using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.PoolTickets.Commands.UpdatePoolTicketPrice;

// ── Command 
public record UpdatePoolTicketPriceCommand(
    int PoolId,
    int TicketTypeId,
    decimal? Price
) : IRequest<SuccessResponse>;

// ── Handler 
public class UpdatePoolTicketPriceCommandHandler
    : IRequestHandler<UpdatePoolTicketPriceCommand, SuccessResponse>
{
    private readonly IUnitOfWork _uow;

    public UpdatePoolTicketPriceCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(
        UpdatePoolTicketPriceCommand request, CancellationToken ct)
    {
        var pt = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolTicketType>().Query()
                .Where(x => x.PoolId       == request.PoolId
                         && x.TicketTypeId == request.TicketTypeId), ct)
            ?? throw new NotFoundException(
                "PoolTicketType",
                $"Pool {request.PoolId} – TicketType {request.TicketTypeId}");

        if (pt.Price != request.Price)
        {
            await _uow.Repository<PoolTicketPriceHistory>().AddAsync(new PoolTicketPriceHistory
            {
                PoolTicketTypeId = pt.PoolTicketTypeId,
                OldCustomPrice = pt.Price, // Giá cũ
                NewCustomPrice = request.Price,  // Giá mới
                ModifiedAt = System.DateTime.UtcNow,
                ModifiedByUserName = "Manager" // Lấy từ HttpContext nếu có
            }, ct);
        }

        pt.Price = request.Price;
        _uow.Repository<PoolTicketType>().Update(pt);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse
        {
            Message = request.Price.HasValue 
                ? $"Đã cập nhật giá áp dụng thành {request.Price.Value:N0}đ."
                : "Đã xóa, hệ thống sẽ sử dụng Giá gốc."
        };
    }
}

// ── Validator 
public class UpdatePoolTicketPriceCommandValidator
    : AbstractValidator<UpdatePoolTicketPriceCommand>
{
    public UpdatePoolTicketPriceCommandValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá vé phải lớn hơn 0.")
            .When(x => x.Price.HasValue);
    }
}
