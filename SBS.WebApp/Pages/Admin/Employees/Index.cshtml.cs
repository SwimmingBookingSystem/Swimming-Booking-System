using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Admin.Employees;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "Quản lý Nhân sự";
        ViewData["Breadcrumb"] = "Quản trị / Nhân sự";
    }
}
