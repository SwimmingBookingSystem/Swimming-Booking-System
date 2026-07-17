using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Admin.Contacts;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "Quản lý Liên hệ";
        ViewData["Breadcrumb"] = "Quản trị / Liên hệ khách hàng";
    }
}
