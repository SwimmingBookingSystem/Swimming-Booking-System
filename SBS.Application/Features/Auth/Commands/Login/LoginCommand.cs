using MediatR;
using SBS.Application.Common.Dtos.Auth;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<AuthResultDto>
{
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.UserName, request.Password, cancellationToken);
    }
}
