using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Contact
{
    public int ContactId { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool? IsResolved { get; set; }

    // Navigation properties
    public virtual ICollection<ContactResponse> ContactResponses { get; set; } = new HashSet<ContactResponse>();
}
