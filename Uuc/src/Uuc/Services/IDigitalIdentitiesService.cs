using Uuc.Models;

namespace Uuc.Services;

public interface IDigitalIdentitiesService
{
	Task<IList<DigitalIdentity>?> ListAllAsync(CancellationToken cancellationToken = default);
}
