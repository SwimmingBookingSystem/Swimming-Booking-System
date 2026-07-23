using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Customer.Bookings;

public class PaymentResultModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PaymentResultModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public string? Code { get; set; }
    public string? Status { get; set; }
    public string? OrderCode { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsCanceled { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string code, string id, bool cancel, string status, string orderCode)
    {
        Code = code;
        Status = status;
        OrderCode = orderCode;
        IsCanceled = cancel;

        // Query parameters are display hints only. Never mark a booking as paid
        // until the API has verified the order directly with PayOS.
        var payOSReportedSuccess = code == "00" && !cancel && status == "PAID";

        if (payOSReportedSuccess && int.TryParse(orderCode, out var paidBookingId))
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7179";
            var token = User.FindFirst("AccessToken")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                ErrorMessage = "Phiên đăng nhập không còn hợp lệ. Vui lòng đăng nhập lại rồi mở lịch sử đặt vé để kiểm tra thanh toán.";
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync(
                    $"{apiBaseUrl}/api/customer-bookings/{paidBookingId}/confirm-payment",
                    null);

                IsSuccess = response.IsSuccessStatusCode;
                if (!IsSuccess)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    ErrorMessage = error?.Message
                        ?? "Hệ thống chưa thể xác nhận giao dịch với PayOS. Vui lòng tải lại trang sau ít phút.";
                }
            }
        }
        else
        {
            IsSuccess = false;
        }

        if (cancel && int.TryParse(orderCode, out int bookingId))
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7179";
            var token = User.FindFirst("AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                await client.PostAsync($"{apiBaseUrl}/api/customer-bookings/{bookingId}/cancel", null);
            }
        }
    }

    private sealed class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}
