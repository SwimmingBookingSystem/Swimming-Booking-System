using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
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

    public async Task OnGetAsync(string code, string id, bool cancel, string status, string orderCode)
    {
        Code = code;
        Status = status;
        OrderCode = orderCode;
        IsCanceled = cancel;

        // "00" indicates success in PayOS
        IsSuccess = (code == "00" && !cancel && status == "PAID");

        if (cancel && int.TryParse(orderCode, out int bookingId))
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7179";
            
            var token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            await client.PostAsync($"{apiBaseUrl}/api/customer-bookings/{bookingId}/cancel", null);
        }
    }
}
