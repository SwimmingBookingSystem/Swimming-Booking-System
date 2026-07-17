using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand : IRequest<ResultDto>
{
    public string Email { get; init; } = null!;
    public string Otp { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
    public string ConfirmPassword { get; init; } = null!;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResultDto>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ResultDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword, cancellationToken);
    }
}
