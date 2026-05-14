using System;

namespace SBS.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Message { get; private set; }
    public string Type { get; private set; }
    public bool IsRead { get; private set; }

    // Navigation properties
    public virtual AppUser User { get; private set; }

    protected Notification() { }

    public Notification(Guid userId, string message, string type)
    {
        UserId = userId;
        Message = message;
        Type = type;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        UpdateTimestamp();
    }
}
