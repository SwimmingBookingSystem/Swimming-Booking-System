using MediatR;
using SBS.Application.Features.Customer_Bookings.Dtos;
using System.Collections.Generic;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetCustomerWaitlists;

public record GetCustomerWaitlistsQuery : IRequest<List<CustomerWaitlistDto>>;
