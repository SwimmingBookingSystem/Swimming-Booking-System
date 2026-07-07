using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace SBS.WebApp.Pages;

public class ContactModel : PageModel
{
    public string PreFillFullName { get; set; } = "";
    public string PreFillEmail { get; set; } = "";
    public string PreFillPhone { get; set; } = "";

    public void OnGet()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            PreFillFullName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
            PreFillEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            PreFillPhone = User.FindFirst(ClaimTypes.MobilePhone)?.Value ?? "";
        }
    }
}
