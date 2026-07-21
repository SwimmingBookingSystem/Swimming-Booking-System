using MediatR;
using SBS.Application.Common.Dtos.Customer.CustomerViewPoolDetail;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer.CustomerViewPoolDetail.Queries;

public record GetCustomerPoolDetailQuery(int PoolId) : IRequest<CustomerPoolDetailDto>;
