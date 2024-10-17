using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Domains;

public class DomainsMockService(IServicesMockData mockServiceData) : IDomainsService
{
	public Task<ObservableCollection<DomainModel>> GetAccountDomainsAsync() =>
		Task.FromResult(new ObservableCollection<DomainModel>(mockServiceData.Domains));

	public Task<ObservableCollection<DomainModel>> SearchAuthorsAsync(string search)
	{
		var items = mockServiceData.Domains.Where(x => x.Name.Contains(search)).ToList();
		var result = new ObservableCollection<DomainModel>(items);
		return Task.FromResult(result);
	}

	public Task<ObservableCollection<DomainModel>> FilterAuthorsAsync(AuthorStatus status) =>
		Task.FromResult(new ObservableCollection<DomainModel>(
			mockServiceData.Domains.Where(x => x.Status == status)));

	public Task RegisterAuthorAsync(DomainModel author)
	{
		throw new NotImplementedException();
	}

	public Task RenewAuthorAsync(DomainModel author)
	{
		throw new NotImplementedException();
	}

	public Task MakeBidAsync(DomainModel author, decimal amount)
	{
		throw new NotImplementedException();
	}
}
