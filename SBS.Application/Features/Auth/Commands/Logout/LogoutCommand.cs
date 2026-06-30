using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest<ResultDto>
{
    public string RefreshToken { get; init; } = null!;
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ResultDto>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ResultDto> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
    }
}
