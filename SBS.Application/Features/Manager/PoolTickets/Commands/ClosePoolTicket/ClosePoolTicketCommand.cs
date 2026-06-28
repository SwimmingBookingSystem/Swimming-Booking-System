using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
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
    private readonly IUnitOfWork _uow;

    public ClosePoolTicketCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(ClosePoolTicketCommand request, CancellationToken ct)
    {
        var pt = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolTicketType>().Query()
                .Where(x => x.PoolId       == request.PoolId
                         && x.TicketTypeId == request.TicketTypeId), ct)
            ?? throw new NotFoundException(
                "PoolTicketType",
                $"Pool {request.PoolId} – TicketType {request.TicketTypeId}");

        if (pt.Status == "Inactive")
            throw new BadRequestException("Vé tại bể bơi này đã ở trạng thái Inactive.");

        pt.Status = "Inactive";
        _uow.Repository<PoolTicketType>().Update(pt);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã ngừng áp dụng vé tại bể bơi." };
    }
}
