using System;

namespace SBS.Domain.Entities;

public class PoolSchedule : BaseEntity
{
    public Guid PoolId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public decimal Price { get; private set; }
    public int MaxCapacity { get; private set; }

    // Navigation properties
    public virtual SwimmingPool Pool { get; private set; }

    protected PoolSchedule() { }

    public PoolSchedule(Guid poolId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal price, int maxCapacity)
    {
        PoolId = poolId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        Price = price;
        MaxCapacity = maxCapacity;
    }
}
