using Microsoft.EntityFrameworkCore;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Domain.Entities;
using SBS.Infrastructure.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.CustomerBookings.Repositories;

public class PoolSlotBookingRepository : IPoolSlotBookingRepository
{
    private readonly ApplicationDbContext _context;

    public PoolSlotBookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PoolSlot?> GetPoolSlotWithLockAsync(int poolSlotId, CancellationToken cancellationToken)
    {
        // Execute raw SQL to place an UPDLOCK and ROWLOCK on the selected PoolSlot
        // This prevents other transactions from modifying or locking this row until the current transaction completes
        var slot = await _context.PoolSlots
            .FromSqlRaw("SELECT * FROM PoolSlots WITH (UPDLOCK, ROWLOCK) WHERE PoolSlotId = {0}", poolSlotId)
            .FirstOrDefaultAsync(cancellationToken);

        return slot;
    }
}
