using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Pages.Domains;
using UC.Umc.Popups;
using UC.Umc.Services;
using UC.Umc.Services.Domains;

namespace UC.Umc.ViewModels.Domains;

public partial class DomainsViewModel(IDomainsService service, ILogger<DomainsViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private string _filter;

	[ObservableProperty]
	private DomainModel _selectedItem;

	[ObservableProperty]
	private ObservableCollection<DomainModel> _domains = new();

	[ObservableProperty]
	private ObservableCollection<string> _domainsFilter = new();

	[RelayCommand]
	public async Task SearchAuthorsAsync()
	{
		try
		{
			InitializeLoading();

			// Search authors
			var authors = await service.SearchAuthorsAsync(Filter);

			Domains.Clear();
			Domains.AddRange(authors);

			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchAuthorsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	public async Task FilterAuthorsAsync(string status)
	{
		try
		{
			Guard.IsNotNull(status);

			InitializeLoading();

			// Filter authors
			ObservableCollection<DomainModel> domains;
			if (status != string.Empty && status != "All")
			{
				var authorStatus = (AuthorStatus) Enum.Parse(typeof(AuthorStatus), status);
				domains = await service.FilterAuthorsAsync(authorStatus);
			}
			else
			{
				domains = await service.GetAccountDomainsAsync();
			}

			Domains.ReplaceAll(domains);

			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchAuthorsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task OpenOptionsAsync(DomainModel author)
	{
		try
		{
			if (author.Status != AuthorStatus.Reserved)
			{
				await ShowPopup(new DomainOptionsPopup(author));
			}
		}
		catch (ArgumentException ex)
		{
			_logger.LogError("OpenOptionsAsync: Author cannot be null, Error: {Message}", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenOptionsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task OpenAuthorDetailsAsync(DomainModel author) =>
		await Navigation.GoToAsync(nameof(DomainDetailsPage),
			new Dictionary<string, object>() { { QueryKeys.AUTHOR, author } });

	[RelayCommand]
	private async Task SortAuthorsAsync()
	{
		// Authors.OrderBy(x => x.Name);
		await Task.Delay(10);
	}

	public async Task InitializeAsync()
	{
		DomainsFilter = DefaultDataMock.AuthorsFilter;
		InitializeLoading();

		ObservableCollection<DomainModel> domains = await service.GetAccountDomainsAsync();
		Domains.ReplaceAll(domains);

		FinishLoading();
	}
}
