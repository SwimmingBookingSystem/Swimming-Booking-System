using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Manager.Pricing
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Bảng giá bể bơi";
            ViewData["Breadcrumb"] = "SBS / Bảng giá";
        }
    }
}
