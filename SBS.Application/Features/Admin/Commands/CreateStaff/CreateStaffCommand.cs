using MediatR;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.CreateStaff;

public record CreateStaffCommand : IRequest<ResultDto>
{
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? Gender { get; init; }
    public DateOnly? Dob { get; init; }
    public string Password { get; init; } = null!;
}

public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, ResultDto>
{
    private readonly IAdminService _adminService;

    public CreateStaffCommandHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<ResultDto> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var dto = new CreateUserDto
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Gender = request.Gender,
            Dob = request.Dob,
            Password = request.Password
        };

        return await _adminService.CreateStaffAsync(dto, cancellationToken);
    }
}
