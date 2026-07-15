using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.VerifyResetOtp;

public record VerifyResetOtpCommand : IRequest<ResultDto>
{
    public string Email { get; init; } = null!;
    public string Otp { get; init; } = null!;
}

public class VerifyResetOtpCommandHandler : IRequestHandler<VerifyResetOtpCommand, ResultDto>
{
    private readonly IAuthService _authService;

    public VerifyResetOtpCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ResultDto> Handle(VerifyResetOtpCommand request, CancellationToken cancellationToken)
    {
        return await _authService.VerifyResetOtpAsync(request.Email, request.Otp, cancellationToken);
    }
}
