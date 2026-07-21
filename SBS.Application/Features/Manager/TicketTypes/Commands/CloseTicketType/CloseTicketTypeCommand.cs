using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Services.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.TicketTypes.Commands.CloseTicketType;

public record CloseTicketTypeCommand(int TicketTypeId) : IRequest<SuccessResponse>;

public class CloseTicketTypeCommandHandler
    : IRequestHandler<CloseTicketTypeCommand, SuccessResponse>
{
    private readonly ITicketManagementService _ticketService;

    public CloseTicketTypeCommandHandler(ITicketManagementService ticketService) 
    {
        _ticketService = ticketService;
    }

    public async Task<SuccessResponse> Handle(CloseTicketTypeCommand request, CancellationToken ct)
    {
        await _ticketService.CloseTicketTypeAsync(request.TicketTypeId, ct);
        return new SuccessResponse { Message = "Đã ngừng kinh doanh loại vé trên toàn hệ thống." };
    }
}
