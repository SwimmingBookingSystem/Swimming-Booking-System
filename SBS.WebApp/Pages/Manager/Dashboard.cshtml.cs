using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Manager
{
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Breadcrumb"] = "SBS / Tổng quan";
        }
    }
}
