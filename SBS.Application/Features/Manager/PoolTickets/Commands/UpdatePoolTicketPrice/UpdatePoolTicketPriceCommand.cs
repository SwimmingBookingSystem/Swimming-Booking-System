using FluentValidation;
using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Services.Interfaces;
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
    private readonly ITicketManagementService _ticketService;

    public UpdatePoolTicketPriceCommandHandler(ITicketManagementService ticketService)
    {
        _ticketService = ticketService;
    }

    public async Task<SuccessResponse> Handle(
        UpdatePoolTicketPriceCommand request, CancellationToken ct)
    {
        await _ticketService.UpdatePoolTicketPriceAsync(request.PoolId, request.TicketTypeId, request.Price, ct);

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
