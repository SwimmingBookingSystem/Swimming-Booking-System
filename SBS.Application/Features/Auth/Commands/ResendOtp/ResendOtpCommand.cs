using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.ResendOtp;

public record ResendOtpCommand : IRequest<ResultDto>
{
    public string Email { get; init; } = null!;
}

public class ResendOtpCommandHandler : IRequestHandler<ResendOtpCommand, ResultDto>
{
    private readonly IAuthService _authService;

    public ResendOtpCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ResultDto> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ResendOtpAsync(request.Email, cancellationToken);
    }
}
