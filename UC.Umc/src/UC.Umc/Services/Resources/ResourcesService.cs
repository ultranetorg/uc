using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Resources;

public class ResourcesService : IResourcesService
{
	public Task<ObservableCollection<ResourceModel>> GetAllProductsAsync()
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<ResourceModel>> GetAuthorProductsAsync(string authorName)
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<ResourceModel>> SearchProductsAsync(string search)
	{
		throw new NotImplementedException();
	}
}
