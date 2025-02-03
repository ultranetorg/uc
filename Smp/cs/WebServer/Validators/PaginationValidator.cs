using System.Runtime.CompilerServices;

namespace Uccs.Smp;

public class PaginationValidator : IPaginationValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(PaginationRequest pagination)
	{
		if (pagination.Page.HasValue && pagination.Page.Value < 0)
		{
			throw new InvalidPaginationParametersException();
		}
		if (pagination.PageSize.HasValue && !Pagination.AllowedPageSizes.Contains(pagination.PageSize.Value))
		{
			throw new InvalidPaginationParametersException();
		}
	}
}
