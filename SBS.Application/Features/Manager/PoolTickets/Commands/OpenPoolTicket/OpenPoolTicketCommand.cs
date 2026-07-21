using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Services.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.PoolTickets.Commands.OpenPoolTicket;

public record OpenPoolTicketCommand(int PoolId, int TicketTypeId) : IRequest<SuccessResponse>;

public class OpenPoolTicketCommandHandler
    : IRequestHandler<OpenPoolTicketCommand, SuccessResponse>
{
    private readonly ITicketManagementService _ticketService;

    public OpenPoolTicketCommandHandler(ITicketManagementService ticketService)
    {
        _ticketService = ticketService;
    }

    public async Task<SuccessResponse> Handle(OpenPoolTicketCommand request, CancellationToken ct)
    {
        await _ticketService.OpenPoolTicketAsync(request.PoolId, request.TicketTypeId, ct);
        return new SuccessResponse { Message = "Đã mở áp dụng vé cho bể bơi này." };
    }
}
