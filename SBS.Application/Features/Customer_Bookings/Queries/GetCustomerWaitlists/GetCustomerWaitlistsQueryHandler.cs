using System;
using MediatR;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetCustomerWaitlists;

public class GetCustomerWaitlistsQueryHandler : IRequestHandler<GetCustomerWaitlistsQuery, PagedResultDto<CustomerWaitlistDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerWaitlistsQueryHandler(IReadOnlyUnitOfWork readOnlyUnitOfWork, ICurrentUserService currentUserService)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResultDto<CustomerWaitlistDto>> Handle(GetCustomerWaitlistsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return new PagedResultDto<CustomerWaitlistDto> { Page = pageNumber, PageSize = pageSize };
        }

        if (!Guid.TryParse(currentUserId, out Guid userIdGuid))
        {
            return new PagedResultDto<CustomerWaitlistDto> { Page = pageNumber, PageSize = pageSize };
        }

        var waitlistsQuery = _readOnlyUnitOfWork.Repository<SBS.Domain.Entities.WaitlistEntry>()
            .Query()
            .Include(w => w.PoolSlot)
                .ThenInclude(ps => ps.Pool)
            .Where(w => w.UserId == userIdGuid);

        var totalCount = await waitlistsQuery.CountAsync(cancellationToken);
        var waitlists = await waitlistsQuery
            .OrderByDescending(w => w.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        return new PagedResultDto<CustomerWaitlistDto>
        {
            Items = waitlists,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}
