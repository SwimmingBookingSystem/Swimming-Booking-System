using System;

namespace SBS.Application.Features.Customer_Bookings.Policies;

public static class BookingTimePolicy
{
    public const int MinimumRemainingSwimmingMinutes = 30;

    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public static (DateOnly Date, TimeSpan Time) GetVietnamDateAndTime(DateTime utcNow)
    {
        var normalizedUtcNow = utcNow.Kind == DateTimeKind.Utc
            ? utcNow
            : utcNow.ToUniversalTime();
        var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(normalizedUtcNow, VietnamTimeZone);

        return (DateOnly.FromDateTime(vietnamNow), vietnamNow.TimeOfDay);
    }

    public static bool IsBookingClosed(
        DateOnly slotDate,
        TimeSpan slotEndTime,
        DateOnly currentDate,
        TimeSpan currentTime)
    {
        if (slotDate < currentDate)
        {
            return true;
        }

        if (slotDate > currentDate)
        {
            return false;
        }

        var bookingCutoff = slotEndTime - TimeSpan.FromMinutes(MinimumRemainingSwimmingMinutes);
        return currentTime >= bookingCutoff;
    }

    public static bool HasSlotEnded(
        DateOnly slotDate,
        TimeSpan slotEndTime,
        DateOnly currentDate,
        TimeSpan currentTime)
    {
        return slotDate < currentDate ||
               (slotDate == currentDate && currentTime >= slotEndTime);
    }

    public static DateTime GetBookingCutoffUtc(DateOnly slotDate, TimeSpan slotEndTime)
    {
        var vietnamCutoff = DateTime.SpecifyKind(
            slotDate.ToDateTime(
                TimeOnly.FromTimeSpan(slotEndTime - TimeSpan.FromMinutes(MinimumRemainingSwimmingMinutes))),
            DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(vietnamCutoff, VietnamTimeZone);
    }
}