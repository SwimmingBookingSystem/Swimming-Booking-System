using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Admin.Dashboard;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "Tổng quan";
        ViewData["Breadcrumb"] = "Quản trị / Tổng quan";
    }
}
