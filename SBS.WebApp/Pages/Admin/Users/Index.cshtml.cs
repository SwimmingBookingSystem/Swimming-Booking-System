using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Features.Admin.Commands.LockUser;
using SBS.Application.Features.Admin.Commands.UnlockUser;
using SBS.Application.Features.Admin.Queries.GetUsers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly ISender _mediator;

    public IndexModel(ISender mediator)
    {
        _mediator = mediator;
    }

    public List<UserListDto> Users { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Users = await _mediator.Send(new GetUsersQuery());
    }

    public async Task<IActionResult> OnPostLockAsync(Guid userId)
    {
        var result = await _mediator.Send(new LockUserCommand(userId));
        if (!result.Succeeded)
            StatusMessage = string.Join("; ", result.Errors);
        else
            StatusMessage = "Khóa tài khoản thành công.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnlockAsync(Guid userId)
    {
        var result = await _mediator.Send(new UnlockUserCommand(userId));
        if (!result.Succeeded)
            StatusMessage = string.Join("; ", result.Errors);
        else
            StatusMessage = "Mở khóa tài khoản thành công.";
        return RedirectToPage();
    }
}
