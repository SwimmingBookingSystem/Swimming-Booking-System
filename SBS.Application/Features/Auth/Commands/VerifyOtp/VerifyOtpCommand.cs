using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.VerifyOtp;

public record VerifyOtpCommand : IRequest<ResultDto>
{
    public string Email { get; init; } = null!;
    public string Otp { get; init; } = null!;
}

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, ResultDto>
{
    private readonly IAuthService _authService;

    public VerifyOtpCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        return await _authService.VerifyOtpAsync(request.Email, request.Otp, cancellationToken);
    }
}
