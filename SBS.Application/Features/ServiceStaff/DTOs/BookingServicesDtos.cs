using System;
using System.Collections.Generic;

namespace SBS.Application.Features.ServiceStaff.DTOs;

public class BookingServicesDto
{
    public int BookingId { get; set; }
    public string? CustomerName { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public List<ServiceStaffBookingServiceItemDto> Services { get; set; } = new();
}

public class ServiceStaffBookingServiceItemDto
{
    public int BookingServiceId { get; set; }
    public int PoolServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalServicePrice { get; set; }
}
