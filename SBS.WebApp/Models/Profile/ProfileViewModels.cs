using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SBS.WebApp.Models.Profile
{
    public class UserProfileDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = null!;
        public DateOnly? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class CustomerBookingHistoryDto
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = null!;
        public string PoolName { get; set; } = null!;
        public DateOnly SlotDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string? QrCodeData { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
