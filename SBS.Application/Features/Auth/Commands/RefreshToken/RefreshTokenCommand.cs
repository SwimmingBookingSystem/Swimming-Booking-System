using MediatR;
using SBS.Application.Common.Dtos.Auth;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<AuthResultDto>
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, cancellationToken);
    }
}
