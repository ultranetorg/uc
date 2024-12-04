using System.Diagnostics.CodeAnalysis;
using Uuc.Models;

namespace Uuc.Services;

public interface IAccountsService
{
	Task<bool> AnyByName([NotEmpty] string name);

	Task CreateAsync([NotEmpty] string name);

	Task<IList<Account>?> ListAllAsync(CancellationToken cancellationToken = default);
}
