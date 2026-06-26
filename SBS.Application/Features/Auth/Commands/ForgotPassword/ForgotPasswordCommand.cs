using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<ResultDto>
{
    public string Email { get; init; } = null!;
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ResultDto>
{
    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ResultDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ForgotPasswordAsync(request.Email, cancellationToken);
    }
}
