using SBS.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Interfaces;

public interface IPoolSlotBookingRepository
{
    // Uses raw SQL to acquire an UPDLOCK on the row to prevent race conditions
    Task<PoolSlot?> GetPoolSlotWithLockAsync(int poolSlotId, CancellationToken cancellationToken);
}
