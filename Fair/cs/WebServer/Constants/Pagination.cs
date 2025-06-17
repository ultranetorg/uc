namespace Uccs.Fair;

public static class Pagination
{
	public const int DefaultPageSize = 20;

	public const int PageSize30 = 30;

	public const int PagesCountMax = 10;

	public static int[] AllowedPageSizes = [ 5, 10, 20, 50 ];

	public const int PublicationsSearchPageSize = 30;
}
