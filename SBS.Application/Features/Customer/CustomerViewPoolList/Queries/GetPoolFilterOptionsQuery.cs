using MediatR;
using SBS.Application.Common.Dtos.Customer.CustomerViewPoolList;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer.CustomerViewPoolList.Queries;

public record GetPoolFilterOptionsQuery() : IRequest<PoolFilterOptionsDto>;

public class TimeOptionDto
{
    public System.TimeSpan Time { get; set; }
}
