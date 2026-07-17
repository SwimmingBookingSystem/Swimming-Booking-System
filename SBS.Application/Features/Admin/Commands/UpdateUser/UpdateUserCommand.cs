using MediatR;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<ResultDto>
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? Gender { get; init; }
    public DateOnly? Dob { get; init; }
    public int? PoolId { get; init; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ResultDto>
{
    private readonly IAdminService _adminService;

    public UpdateUserCommandHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<ResultDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = new UpdateUserDto
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Gender = request.Gender,
            Dob = request.Dob,
            PoolId = request.PoolId
        };

        return await _adminService.UpdateUserAsync(request.UserId, dto, cancellationToken);
    }
}
