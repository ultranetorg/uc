namespace Uccs.Fair;

public class CategoryPublicationsModel(Category category) : CategoryBaseModel(category)
{
	public CategoryType Type { get; set; } = category.Type;

	public List<PublicationExtendedModel> Publications { get; set; }
}
