using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; private set; }
    public string AvatarUrl { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string Gender { get; private set; }
    public string Address { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; private set; }
    public virtual ICollection<Review> Reviews { get; private set; }
    public virtual ICollection<CheckIn> CheckInsAsReceptionist { get; private set; }
    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; private set; }
    public virtual ICollection<StaffShift> Shifts { get; private set; }
    public virtual ICollection<Notification> Notifications { get; private set; }

    // Required by EF Core
    protected AppUser() 
    {
        Bookings = new HashSet<Booking>();
        Reviews = new HashSet<Review>();
        CheckInsAsReceptionist = new HashSet<CheckIn>();
        MaintenanceRequests = new HashSet<MaintenanceRequest>();
        Shifts = new HashSet<StaffShift>();
        Notifications = new HashSet<Notification>();
    }

    public AppUser(string userName, string email, string fullName) : this()
    {
        Id = Guid.NewGuid();
        UserName = userName;
        Email = email;
        FullName = fullName;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
