using MediatR;
using SBS.Application.Common.Dtos;
using SBS.Application.Features.Customer_Bookings.Dtos;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetCustomerWaitlists;

public record GetCustomerWaitlistsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResultDto<CustomerWaitlistDto>>;