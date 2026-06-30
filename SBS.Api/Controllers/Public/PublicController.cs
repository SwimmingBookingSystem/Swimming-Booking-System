using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SBS.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Public;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly ReadDbContext _readContext;

    public PublicController(ReadDbContext readContext)
    {
        _readContext = readContext;
    }

    [HttpGet("homepage")]
    public async Task<IActionResult> GetHomepageData()
    {
        var totalPools = await _readContext.Pools.CountAsync(p => p.Status == "Active");
        var totalCustomers = await _readContext.Users.CountAsync();
        var totalBookings = await _readContext.Bookings.CountAsync();
        var avgRating = await _readContext.Feedbacks
            .AverageAsync(f => (double?)f.Rating) ?? 0;

        var pools = await _readContext.Pools
            .Where(p => p.Status == "Active")
            .OrderBy(p => p.PoolName)
            .Take(6)
            .Select(p => new
            {
                p.PoolId,
                p.PoolName,
                p.Address,
                p.Description,
                p.OpeningTime,
                p.ClosingTime,
                CoverImageUrl = p.PoolImages
                    .Where(img => img.IsCover)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault()
                    ?? p.PoolImages.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).FirstOrDefault(),
                MinPrice = p.PoolTicketTypes
                    .Where(pt => pt.Status == "Active")
                    .Min(pt => (decimal?)pt.Price)
            })
            .ToListAsync();

        var result = new
        {
            Stats = new
            {
                TotalPools = totalPools,
                TotalCustomers = totalCustomers,
                TotalBookings = totalBookings,
                AverageRating = System.Math.Round(avgRating, 1)
            },
            Pools = pools.Select(p => new
            {
                p.PoolId,
                p.PoolName,
                p.Address,
                p.Description,
                p.CoverImageUrl,
                OpeningTime = p.OpeningTime.ToString(@"hh\:mm"),
                ClosingTime = p.ClosingTime.ToString(@"hh\:mm"),
                MinPrice = p.MinPrice
            })
        };

        return Ok(result);
    }
}
