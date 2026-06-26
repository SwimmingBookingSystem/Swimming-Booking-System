using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<ResultDto>
{
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string ConfirmPassword { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public DateOnly? Dob { get; init; }
    public string? Address { get; init; }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ResultDto>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(
            request.UserName,
            request.Email,
            request.Password,
            request.FullName,
            request.PhoneNumber,
            request.Dob,
            request.Address,
            cancellationToken);
    }
}
