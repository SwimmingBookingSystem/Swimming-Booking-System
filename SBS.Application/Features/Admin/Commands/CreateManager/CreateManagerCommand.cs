using MediatR;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.CreateManager;

public record CreateManagerCommand : IRequest<ResultDto>
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

public class CreateManagerCommandHandler : IRequestHandler<CreateManagerCommand, ResultDto>
{
    private readonly IAdminService _adminService;

    public CreateManagerCommandHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<ResultDto> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
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

        return await _adminService.CreateManagerAsync(dto, cancellationToken);
    }
}
