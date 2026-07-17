using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SBS.WebApp.Pages.Admin.Employees;

public class CreateStaffModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "Thêm Nhân viên";
        ViewData["Breadcrumb"] = "Quản trị / Nhân sự / Thêm nhân viên";
    }
}
