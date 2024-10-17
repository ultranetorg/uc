using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Domains;

public sealed class AuthorsService : IDomainsService
{
	public Task<ObservableCollection<DomainModel>> GetAccountDomainsAsync()
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<DomainModel>> SearchAuthorsAsync(string search)
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<DomainModel>> FilterAuthorsAsync(AuthorStatus status)
	{
		throw new NotImplementedException();
	}

	public Task MakeBidAsync(DomainModel author, decimal amount)
	{
		throw new NotImplementedException();
	}

	public Task RegisterAuthorAsync(DomainModel author)
	{
		throw new NotImplementedException();
	}

	public Task RenewAuthorAsync(DomainModel author)
	{
		throw new NotImplementedException();
	}
}
