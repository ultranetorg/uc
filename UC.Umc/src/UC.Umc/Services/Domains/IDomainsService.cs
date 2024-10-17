using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Domains;

public interface IDomainsService
{
	Task<ObservableCollection<DomainModel>> GetAccountDomainsAsync();

	Task<ObservableCollection<DomainModel>> SearchAuthorsAsync(string search);

	Task<ObservableCollection<DomainModel>> FilterAuthorsAsync(AuthorStatus status);

	Task RegisterAuthorAsync(DomainModel author);

	Task RenewAuthorAsync(DomainModel author);

	Task MakeBidAsync(DomainModel author, decimal amount);
}
