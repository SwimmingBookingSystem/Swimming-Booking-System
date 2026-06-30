using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Admin.Employees;

public class CreateManagerModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "Thêm Quản lý";
        ViewData["Breadcrumb"] = "Quản trị / Nhân sự / Thêm quản lý";
    }
}
