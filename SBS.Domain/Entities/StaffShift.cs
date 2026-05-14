using System;

namespace SBS.Domain.Entities;

public class StaffShift : BaseEntity
{
    public Guid StaffId { get; private set; }
    public DateTime Date { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string Area { get; private set; }

    // Navigation properties
    public virtual AppUser Staff { get; private set; }

    protected StaffShift() { }

    public StaffShift(Guid staffId, DateTime date, TimeSpan startTime, TimeSpan endTime, string area)
    {
        StaffId = staffId;
        Date = date.Date;
        StartTime = startTime;
        EndTime = endTime;
        Area = area;
    }
}
