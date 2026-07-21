using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Dtos.Customer.CustomerViewPoolList;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SBS.Application.Features.Customer.CustomerViewPoolList.Queries;

public record GetCustomerPoolsQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchName = null,
    string? Address = null,
    TimeSpan? OpeningTime = null,
    TimeSpan? ClosingTime = null,
    int? MinCapacity = null,
    int? MaxCapacity = null
) : IRequest<PagedResponse<CustomerPoolDto>>;
