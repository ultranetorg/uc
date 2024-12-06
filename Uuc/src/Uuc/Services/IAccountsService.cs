using System.Diagnostics.CodeAnalysis;
using Uuc.Models.Accounts;

namespace Uuc.Services;

public interface IAccountsService
{
	Task<bool> AnyByName([NotEmpty] string name);

	Task CreateAsync([NotEmpty] string name);

	Task<IList<Account>?> ListAllAsync(CancellationToken cancellationToken = default);

	Task<Account?> FindAsync([NotEmpty] string address, CancellationToken cancellationToken = default);
}
