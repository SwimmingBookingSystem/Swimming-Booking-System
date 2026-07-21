namespace SBS.Application.Common;

/// <summary>
/// Tập trung toàn bộ booking status strings để tránh typo và đảm bảo đồng bộ
/// giữa Customer và Staff. KHÔNG thay đổi giá trị string mà không update DB.
/// </summary>
public static class BookingStatus
{
    public const string PendingPayment = "PendingPayment";
    public const string Paid           = "Paid";
    public const string CheckIn        = "CheckIn";
    public const string Completed      = "Completed";
    public const string Cancelled      = "Cancelled";
    public const string Failed         = "Failed";
    public const string Refunded       = "Refunded";

    /// <summary>Chuyển status code sang nhãn tiếng Việt hiển thị trên frontend.</summary>
    public static string ToDisplayName(string? status) => status switch
    {
        PendingPayment => "Chờ thanh toán",
        Paid           => "Đã thanh toán",
        CheckIn        => "Đã check-in",
        Completed      => "Hoàn thành",
        Cancelled      => "Đã hủy",
        Failed         => "Thất bại",
        Refunded       => "Đã hoàn tiền",
        _              => status ?? "Không xác định"
    };
}

/// <summary>
/// Status của ContactRequest. Đồng bộ với Customer_Bookings và Admin.
/// </summary>
public static class ContactRequestStatus
{
    public const string Pending  = "Pending";
    public const string Resolved = "Resolved";

    /// <summary>Chuyển status code sang nhãn tiếng Việt hiển thị trên frontend.</summary>
    public static string ToDisplayName(string? status) => status switch
    {
        Pending  => "Chờ xử lý",
        Resolved => "Đã xử lý",
        _        => status ?? "Không xác định"
    };
}

/// <summary>
/// Status của Payment record. Đồng bộ với ProcessPaymentWebhookCommandHandler.
/// </summary>
public static class PaymentStatus
{
    public const string Success = "Success";

    /// <summary>Chuyển status code sang nhãn tiếng Việt hiển thị trên frontend.</summary>
    public static string ToDisplayName(string? status) => status switch
    {
        Success => "Thành công",
        _       => status ?? "Không xác định"
    };
}
