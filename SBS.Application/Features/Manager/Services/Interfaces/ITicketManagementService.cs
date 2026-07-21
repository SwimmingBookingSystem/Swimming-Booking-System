using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Services.Interfaces;

public interface ITicketManagementService
{
    Task ClosePoolTicketAsync(int poolId, int ticketTypeId, CancellationToken ct);
    Task OpenPoolTicketAsync(int poolId, int ticketTypeId, CancellationToken ct);
    Task UpdatePoolTicketPriceAsync(int poolId, int ticketTypeId, decimal? customPrice, CancellationToken ct);

    Task CloseTicketTypeAsync(int ticketTypeId, CancellationToken ct);
    Task OpenTicketTypeAsync(int ticketTypeId, CancellationToken ct);
}
