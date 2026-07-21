using SBS.Application.Common;
using SBS.Application.Common.Dtos.Staff;
using SBS.Domain.Entities;
using System;

namespace SBS.Application.Features.Staff.Commands;

/// <summary>
/// Helper dùng chung cho QrCheckIn và ManualCheckIn để tránh code duplicate.
/// Kiểm tra tính hợp lệ của booking trước khi thực hiện check-in.
/// </summary>
public static class CheckInValidationHelper
{
    /// <summary>
    /// Kiểm tra đầy đủ điều kiện check-in:
    /// trạng thái Paid, đúng ngày, trong khung giờ (cho phép trước 15 phút),
    /// và chưa check-in trước đó.
    /// </summary>
    /// <param name="booking">Booking đã Include PoolSlot và CheckIn.</param>
    /// <param name="localNow">Thời điểm hiện tại theo giờ Việt Nam (UTC+7).</param>
    /// <returns>Tuple (IsValid, ErrorDto) — nếu IsValid = false thì ErrorDto chứa thông báo lỗi.</returns>
    public static (bool IsValid, StaffCheckInResultDto? ErrorDto) Validate(
        Booking booking,
        DateTime localNow)
    {
        // 1. Trạng thái phải là Paid
        if (booking.Status != BookingStatus.Paid)
            return (false, Fail(
                $"Booking không thể check-in. Trạng thái hiện tại: {booking.Status} (yêu cầu: Paid)."));

        // 2. Đúng ngày hôm nay
        var today = DateOnly.FromDateTime(localNow);
        if (booking.BookingDate != today)
            return (false, Fail(
                $"Booking chỉ hợp lệ vào ngày {booking.BookingDate:dd/MM/yyyy}. Hôm nay là {today:dd/MM/yyyy}."));

        // 3. Trong khung giờ hợp lệ (cho phép check-in trước 15 phút, trước khi ca kết thúc)
        var slotStart    = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(booking.PoolSlot.StartTime));
        var slotEnd      = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(booking.PoolSlot.EndTime));
        var allowedStart = slotStart.AddMinutes(-15);

        if (localNow < allowedStart)
            return (false, Fail(
                $"Ca bơi chưa bắt đầu. Bạn chỉ có thể check-in từ {allowedStart:HH:mm} (trước giờ bơi tối đa 15 phút)."));

        if (localNow > slotEnd)
            return (false, Fail(
                $"Ca bơi đã kết thúc vào lúc {TimeOnly.FromTimeSpan(booking.PoolSlot.EndTime):HH:mm}. Không thể check-in."));

        // 4. Chưa check-in trước đó
        if (booking.CheckIn is not null)
            return (false, Fail(
                $"Booking này đã được check-in lúc {booking.CheckIn.CheckInTime:HH:mm dd/MM/yyyy}."));

        return (true, null);
    }

    private static StaffCheckInResultDto Fail(string message) =>
        new() { Succeeded = false, Message = message };
}
