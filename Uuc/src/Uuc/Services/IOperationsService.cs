using System.Diagnostics.CodeAnalysis;
using Uuc.Models.Accounts;

namespace Uuc.Services;

public interface IOperationsService
{
	Task<IList<Operation>?> ListAllAsync(CancellationToken cancellationToken = default);

	Task<IList<Operation>?> ListByAccountAddressAsync([NotEmpty] string accountAddress, CancellationToken cancellationToken = default);
}
