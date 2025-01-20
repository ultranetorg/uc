using System.Runtime.CompilerServices;
using Uccs.Web.Utilities;

namespace Uccs.Fair;

public class PaginationValidator : IPaginationValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(PaginationRequest pagination)
	{
		if (pagination.Page.HasValue)
		{
			If.Value(pagination.Page.Value).IsNegative().Throw(() => new InvalidPaginationParametersException());
		}
		if (pagination.PageSize.HasValue && !Pagination.AllowedPageSizes.Contains(pagination.PageSize.Value))
		{
			throw new InvalidPaginationParametersException();
		}
	}
}
