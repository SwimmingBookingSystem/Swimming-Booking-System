using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Manager.Pools
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Quản lý Bể bơi";
            ViewData["Breadcrumb"] = "SBS / Bể bơi";
        }
    }
}
