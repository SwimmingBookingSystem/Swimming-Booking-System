namespace SBS.Application.Common.Dtos.Staff;

/// <summary>
/// Kết quả trả về sau khi thực hiện check-in (QR hoặc thủ công).
/// Đặt tại Common/Dtos/Staff để QrCheckIn và ManualCheckIn dùng chung.
/// </summary>
public record StaffCheckInResultDto
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public string? CustomerName { get; init; }
    public string? SlotTime { get; init; }
}
