namespace Uccs.Fair;

public interface IPaginationValidator
{
	void Validate(PaginationRequest pagination);

	void Validate(int? page);
}
