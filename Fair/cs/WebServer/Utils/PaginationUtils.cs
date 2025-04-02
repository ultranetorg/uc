namespace Uccs.Fair;

public class PaginationUtils
{
	public static (int page, int pageSize) GetPaginationParams(PaginationRequest pagination)
	{
		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		return (page, pageSize);
	}
}
