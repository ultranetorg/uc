using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Resources;

public class ResourcesMockService(IServicesMockData data) : IResourcesService
{
	public Task<ObservableCollection<ResourceModel>> GetAllProductsAsync() =>
		Task.FromResult(new ObservableCollection<ResourceModel>(data.Resources.ToList()));

	public Task<ObservableCollection<ResourceModel>> GetAuthorProductsAsync(string authorName) =>
		Task.FromResult(new ObservableCollection<ResourceModel>(data.Resources.Where(x => x.Author.Name == authorName)));

	public Task<ObservableCollection<ResourceModel>> SearchProductsAsync(string search)
	{
		var items = data.Resources.Where(x => x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToList();
		var result = new ObservableCollection<ResourceModel>(items);
		return Task.FromResult(result);
	}
}
