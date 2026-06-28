using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.TicketTypes.Commands.OpenTicketType;

public record OpenTicketTypeCommand(int TicketTypeId) : IRequest<SuccessResponse>;

public class OpenTicketTypeCommandHandler
    : IRequestHandler<OpenTicketTypeCommand, SuccessResponse>
{
    private readonly IUnitOfWork _uow;

    public OpenTicketTypeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(OpenTicketTypeCommand request, CancellationToken ct)
    {
        var ticket = await _uow.FirstOrDefaultAsync(
            _uow.Repository<TicketType>().Query()
                .Where(t => t.TicketTypeId == request.TicketTypeId), ct)
            ?? throw new NotFoundException(nameof(TicketType), request.TicketTypeId);

        if (ticket.Status == "Active")
            throw new BadRequestException("Loại vé đang ở trạng thái Active, không cần mở lại.");

        ticket.Status = "Active";
        _uow.Repository<TicketType>().Update(ticket);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã kích hoạt lại loại vé." };
    }
}
