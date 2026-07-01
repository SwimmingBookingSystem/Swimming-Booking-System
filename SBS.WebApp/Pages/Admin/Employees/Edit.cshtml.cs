using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Admin.Employees;

public class EditModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    public void OnGet()
    {
        ViewData["Title"] = "Chỉnh sửa Nhân viên";
        ViewData["Breadcrumb"] = "Quản trị / Nhân sự / Chỉnh sửa";
    }
}
