namespace Uccs.Fair;

public enum ErrorCodes
{
	// General
	Base = 1000,

	InvalidEntityId,
	EntityNotFound,

	InvalidPaginationParameters,
	InvalidDepth,
	InvalidSearchQuery,
	InvalidSearchParams,

	InvalidProductVersion,
	InvalidProductType,
	InvalidAccountAddress,
	InvalidCategoryException,
	InvalidEntityParameter,
}
