namespace Uccs.Fair;

public enum ErrorCodes
{
	// General
	Base = 1000,

	InvalidEntityId = Base + 1,
	EntityNotFound = Base + 2,

	InvalidPaginationParameters = Base + 3,
	InvalidDepth = Base + 4,
	InvalidSearchQuery = Base + 5,
	
	InvalidProductVersion = Base + 6
}
