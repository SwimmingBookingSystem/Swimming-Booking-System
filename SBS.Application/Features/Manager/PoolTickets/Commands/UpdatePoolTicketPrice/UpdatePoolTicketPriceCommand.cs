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
    decimal Price
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

        pt.Price = request.Price;
        _uow.Repository<PoolTicketType>().Update(pt);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse
        {
            Message = $"Đã cập nhật giá vé tại bể bơi thành {request.Price:N0}đ."
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
            .GreaterThan(0).WithMessage("Giá vé phải lớn hơn 0.");
    }
}
