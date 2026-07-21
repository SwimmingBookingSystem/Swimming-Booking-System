using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Services.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.PoolTickets.Commands.ClosePoolTicket;

public record ClosePoolTicketCommand(int PoolId, int TicketTypeId) : IRequest<SuccessResponse>;

public class ClosePoolTicketCommandHandler
    : IRequestHandler<ClosePoolTicketCommand, SuccessResponse>
{
    private readonly ITicketManagementService _ticketService;

    public ClosePoolTicketCommandHandler(ITicketManagementService ticketService) 
    {
        _ticketService = ticketService;
    }

    public async Task<SuccessResponse> Handle(ClosePoolTicketCommand request, CancellationToken ct)
    {
        await _ticketService.ClosePoolTicketAsync(request.PoolId, request.TicketTypeId, ct);
        return new SuccessResponse { Message = "Đã ngừng áp dụng vé tại bể bơi." };
    }
}
