using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Admin.Users
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Quản lý Người dùng";
            ViewData["Breadcrumb"] = "Quản trị / Người dùng";
        }
    }
}
