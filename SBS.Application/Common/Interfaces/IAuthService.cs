using SBS.Application.Common.Dtos.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);
}
