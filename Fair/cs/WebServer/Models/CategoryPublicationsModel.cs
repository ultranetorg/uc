namespace Uccs.Fair;

public class CategoryPublicationsModel(Category category) : CategoryBaseModel(category)
{
	public List<PublicationExtendedModel> Publications { get; set; }
}
