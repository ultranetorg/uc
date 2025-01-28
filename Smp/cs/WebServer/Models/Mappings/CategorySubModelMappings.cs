namespace Uccs.Smp;

public class CategorySubModelMappings
{
	public static CategorySubModel MapTo(Category category)
	{
		return new CategorySubModel
		{
			Id = category.Id.ToString(),
			Title = category.Title,
		};
	}
}
