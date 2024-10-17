using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Resources;

public interface IResourcesService
{
	Task<ObservableCollection<ResourceModel>> GetAllProductsAsync();

	Task<ObservableCollection<ResourceModel>> GetAuthorProductsAsync(string authorName);

	Task<ObservableCollection<ResourceModel>> SearchProductsAsync(string search);
}
