namespace SBS.Application.Features.Customer_Bookings.Policies;

public static class BookingTimePolicy
{
    public const int MinimumRemainingSwimmingMinutes = 30;

    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public static (DateOnly Date, TimeSpan Time) GetVietnamDateAndTime(DateTime dateTimeNow)
    {
        // Trả về trực tiếp thời gian local thay vì convert từ UTC
        return (DateOnly.FromDateTime(dateTimeNow), dateTimeNow.TimeOfDay);
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

    public static DateTime GetBookingCutoffUtc(DateOnly slotDate, TimeSpan slotEndTime)
    {
        var localCutoff = slotDate.ToDateTime(
            TimeOnly.FromTimeSpan(slotEndTime - TimeSpan.FromMinutes(MinimumRemainingSwimmingMinutes)),
            DateTimeKind.Unspecified);

        // Đã đổi sang dùng Local Time thay vì UTC nên trả về thẳng localCutoff
        return localCutoff;
    }
}
