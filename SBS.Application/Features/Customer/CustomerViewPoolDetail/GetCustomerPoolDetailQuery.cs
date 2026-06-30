using MediatR;
using SBS.Application.Common.Dtos.Customer.CustomerViewPoolDetail;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer.CustomerViewPoolDetail;

public record GetCustomerPoolDetailQuery(int PoolId) : IRequest<CustomerPoolDetailDto>;

public class GetCustomerPoolDetailQueryHandler : IRequestHandler<GetCustomerPoolDetailQuery, CustomerPoolDetailDto>
{
    private readonly IUnitOfWork _uow;

    public GetCustomerPoolDetailQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CustomerPoolDetailDto> Handle(GetCustomerPoolDetailQuery request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query()
                .Where(p => p.PoolId == request.PoolId && p.Status == "Active"), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        var images = await _uow.ToListAsync(
            _uow.Repository<PoolImage>().Query()
                .Where(img => img.PoolId == pool.PoolId)
                .OrderBy(img => img.SortOrder)
                .Select(img => img.ImageUrl), ct);

        return new CustomerPoolDetailDto
        {
            PoolId = pool.PoolId,
            PoolName = pool.PoolName,
            Address = pool.Address,
            Description = pool.Description,
            OpeningTime = pool.OpeningTime.ToString(@"hh\:mm"),
            ClosingTime = pool.ClosingTime.ToString(@"hh\:mm"),
            Images = images,
            StandardCapacity = pool.StandardCapacity
        };
    }
}
