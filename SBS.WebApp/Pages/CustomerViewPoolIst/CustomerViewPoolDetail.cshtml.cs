using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace SBS.WebApp.Pages.CustomerViewPoolIst
{
    public class CustomerViewPoolDetailModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public CustomerViewPoolDetailModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string ApiBaseUrl { get; private set; } = null!;

        public IActionResult OnGet()
        {
            if (Id <= 0)
            {
                return RedirectToPage("/CustomerViewPoolIst/CustomerViewPoolIst");
            }
            ApiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7179";
            return Page();
        }
    }
}
