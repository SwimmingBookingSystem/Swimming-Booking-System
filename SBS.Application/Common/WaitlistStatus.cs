namespace SBS.Application.Common;

public static class WaitlistStatus
{
    public const string Waiting = "Waiting";
    public const string Offered = "Offered";
    public const string Purchased = "Purchased";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";

    public static string ToDisplayName(string? status) => status switch
    {
        Waiting => "Đang chờ",
        Offered => "Đang chờ thanh toán",
        Purchased => "Đã mua vé",
        Cancelled => "Đã hủy",
        Expired => "Đã hết hạn",
        _ => status ?? "Không xác định"
    };
}
