using System;

namespace SBS.Domain.Entities;

public class Review : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Guid PoolId { get; private set; }
    public string Title { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; }
    public DateTime ReviewDate { get; private set; }

    // Navigation properties
    public virtual AppUser Customer { get; private set; }
    public virtual SwimmingPool Pool { get; private set; }

    protected Review() { }

    public Review(Guid customerId, Guid poolId, string title, int rating, string comment)
    {
        CustomerId = customerId;
        PoolId = poolId;
        Title = title;
        Rating = rating;
        Comment = comment;
        ReviewDate = DateTime.UtcNow;
    }
}
