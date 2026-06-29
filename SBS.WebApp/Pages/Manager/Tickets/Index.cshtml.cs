using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Manager.Tickets
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Quản lý Loại vé";
            ViewData["Breadcrumb"] = "SBS / Loại vé";
        }
    }
}
