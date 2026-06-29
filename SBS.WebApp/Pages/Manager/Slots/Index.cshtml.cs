using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Manager.Slots
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Quản lý Khung giờ (Slots)";
            ViewData["Breadcrumb"] = "SBS / Khung giờ";
        }
    }
}
