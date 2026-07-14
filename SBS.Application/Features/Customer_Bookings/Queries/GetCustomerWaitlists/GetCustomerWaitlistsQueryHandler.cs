using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetCustomerWaitlists;

public class GetCustomerWaitlistsQueryHandler : IRequestHandler<GetCustomerWaitlistsQuery, List<CustomerWaitlistDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerWaitlistsQueryHandler(IReadOnlyUnitOfWork readOnlyUnitOfWork, ICurrentUserService currentUserService)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<CustomerWaitlistDto>> Handle(GetCustomerWaitlistsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return new List<CustomerWaitlistDto>();
        }

        if (!Guid.TryParse(currentUserId, out Guid userIdGuid))
        {
            return new List<CustomerWaitlistDto>();
        }

        var waitlists = await _readOnlyUnitOfWork.Repository<SBS.Domain.Entities.WaitlistEntry>()
            .Query()
            .Include(w => w.PoolSlot)
                .ThenInclude(ps => ps.Pool)
            .Where(w => w.UserId == userIdGuid)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new CustomerWaitlistDto
            {
                WaitlistEntryId = w.WaitlistEntryId,
                PoolId = w.PoolSlot.PoolId,
                PoolName = w.PoolSlot.Pool.PoolName,
                SlotDate = w.PoolSlot.SlotDate,
                StartTime = w.PoolSlot.StartTime,
                EndTime = w.PoolSlot.EndTime,
                Quantity = w.Quantity,
                Status = w.Status,
                CurrentPosition = _readOnlyUnitOfWork.Repository<SBS.Domain.Entities.WaitlistEntry>()
                                    .Query()
                                    .Count(other => other.PoolSlotId == w.PoolSlotId && 
                                                    other.Status == "Waiting" && 
                                                    other.Position <= w.Position),
                TotalWaitlistCount = _readOnlyUnitOfWork.Repository<SBS.Domain.Entities.WaitlistEntry>()
                                    .Query()
                                    .Count(other => other.PoolSlotId == w.PoolSlotId && other.Status == "Waiting"),
                CreatedAt = w.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return waitlists;
    }
}
