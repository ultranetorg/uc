using Uuc.Models;

namespace Uuc.Services;

public interface IAuthorizationsService
{
	Task<IList<Authorization>?> ListAllAsync(CancellationToken cancellationToken = default);
}
