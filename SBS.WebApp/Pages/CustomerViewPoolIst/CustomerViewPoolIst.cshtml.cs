using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace SBS.WebApp.Pages.CustomerViewPoolIst
{
    public class CustomerViewPoolIstModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public CustomerViewPoolIstModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ApiBaseUrl { get; private set; } = null!;

        public void OnGet()
        {
            ApiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7179";
        }
    }
}
