using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.RespondContactRequest;

public record AdminRespondContactRequestCommand : IRequest<ResultDto>
{
    public int ContactRequestId { get; init; }
    public string ResponseMessage { get; init; } = null!;
}

public class AdminRespondContactRequestCommandHandler : IRequestHandler<AdminRespondContactRequestCommand, ResultDto>
{
    private readonly IAdminService _adminService;

    public AdminRespondContactRequestCommandHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<ResultDto> Handle(AdminRespondContactRequestCommand request, CancellationToken cancellationToken)
    {
        return await _adminService.RespondContactRequestAsync(request.ContactRequestId, request.ResponseMessage, cancellationToken);
    }
}
