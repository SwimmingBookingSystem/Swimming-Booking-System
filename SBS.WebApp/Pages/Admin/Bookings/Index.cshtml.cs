using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Admin.Bookings;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "Quản lý Đặt chỗ";
        ViewData["Breadcrumb"] = "Quản trị / Đặt chỗ";
    }
}
