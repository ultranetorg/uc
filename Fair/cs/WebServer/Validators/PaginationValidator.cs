using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class PaginationValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(PaginationRequest pagination)
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(int? page)
	{
		if (page != null && page.Value < 0)
		{
			throw new InvalidPaginationParametersException();
		}
	}
}
