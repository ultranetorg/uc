using Uuc.Models;

namespace Uuc.Services;

public class AuthorizationsService : IAuthorizationsService
{
	public async Task<IList<Authorization>?> ListAllAsync(CancellationToken cancellationToken = default)
	{
		return new List<Authorization>()
		{
			new Authorization
			{
				Source = "google.com"
			},
			new Authorization
			{
				Source = "ultranet.org"
			},
			new Authorization
			{
				Source = "aximion.com"
			}
		};
	}
}
