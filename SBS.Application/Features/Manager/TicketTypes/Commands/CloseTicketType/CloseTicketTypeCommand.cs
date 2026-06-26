using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
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
    private readonly IUnitOfWork _uow;

    public CloseTicketTypeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(CloseTicketTypeCommand request, CancellationToken ct)
    {
        var ticket = await _uow.FirstOrDefaultAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketTypeId == request.TicketTypeId), ct)
            ?? throw new NotFoundException(nameof(TicketType), request.TicketTypeId);

        if (ticket.Status == "Inactive")
            throw new BadRequestException("Loại vé đã ở trạng thái Inactive, không cần đóng lại.");

        ticket.Status = "Inactive";
        _uow.Repository<TicketType>().Update(ticket);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã ngừng kích hoạt loại vé." };
    }
}
