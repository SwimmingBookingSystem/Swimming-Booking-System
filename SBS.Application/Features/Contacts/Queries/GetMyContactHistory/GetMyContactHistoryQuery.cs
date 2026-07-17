using MediatR;
using SBS.Application.Common.Dtos;
using SBS.Application.Features.Contacts.Dtos;
using System;

namespace SBS.Application.Features.Contacts.Queries.GetMyContactHistory;

public record GetMyContactHistoryQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) 
    : IRequest<PagedResultDto<ContactHistoryDto>>;
