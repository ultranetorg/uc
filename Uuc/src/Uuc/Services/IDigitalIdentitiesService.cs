using Uuc.Models;

namespace Uuc.Services;

public interface IDigitalIdentitiesService
{
	Task<IList<DigitalIdentity>?> ListAllAsync(CancellationToken cancellationToken = default);

	Task<DigitalIdentity?> FindAsync(string name, CancellationToken cancellationToken = default);
}
