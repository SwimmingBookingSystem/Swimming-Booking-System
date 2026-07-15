using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetPoolTickets;

public class GetPoolTicketsQueryHandler : IRequestHandler<GetPoolTicketsQuery, List<CustomerPoolTicketDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;

    public GetPoolTicketsQueryHandler(IReadOnlyUnitOfWork readOnlyUnitOfWork)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
    }

    public async Task<List<CustomerPoolTicketDto>> Handle(GetPoolTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _readOnlyUnitOfWork.Repository<PoolTicketType>().Query()
            .AsNoTracking()
            .Include(pt => pt.TicketType)
            .Where(pt => pt.PoolId == request.PoolId && pt.Status == "Active" && pt.TicketType.Status == "Active")
            .Select(pt => new CustomerPoolTicketDto
            {
                PoolTicketTypeId = pt.PoolTicketTypeId,
                TicketName = pt.TicketType.TicketName,
                Category = pt.TicketType.Category,
                Price = pt.Price ?? (pt.TicketType.BasePrice * (1 - pt.TicketType.DiscountPercent / 100m)),
                Description = pt.TicketType.Description
            })
            .ToListAsync(cancellationToken);

        return tickets;
    }
}
