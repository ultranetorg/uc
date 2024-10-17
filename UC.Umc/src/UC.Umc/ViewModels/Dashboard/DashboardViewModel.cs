using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Services.Accounts;
using UC.Umc.Services.Domains;
using UC.Umc.Services.Resources;
using UC.Umc.Services.Transactions;

namespace UC.Umc.ViewModels.Dashboard;

public partial class DashboardViewModel
(
	IAccountsService accountsService,
	IDomainsService domainsService,
	IResourcesService resourcesService,
	ITransactionsService transactionsService,
	ILogger<DashboardViewModel> logger
) : BaseViewModel(logger)
{
	[ObservableProperty]
	private ObservableCollection<AccountModel> _accounts = new();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(AuctionsOutbidded))]
	[NotifyPropertyChangedFor(nameof(AuthorsRenewal))]
	private ObservableCollection<DomainModel> _domains = new();

	[ObservableProperty]
	private ObservableCollection<TransactionModel> _transactions = new();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(NumberOfProducts))]
	private ObservableCollection<ResourceModel> _products = new();
	
	[ObservableProperty]
	public int _numberOfAccounts = 0;

	public int NumberOfProducts => Products?.Count ?? 0;
	public string AuctionsOutbidded => $"{Domains.Count(x => x.Status == AuthorStatus.Auction)} " +
		$"({Domains.Count(x => x.Status == AuthorStatus.Auction && x.BidStatus == BidStatus.Higher)})";
	public string AuthorsRenewal => $"{Domains.Count} ({Domains.Count(x => x.ExpiresSoon)})";

	[RelayCommand]
	public async Task AccountsExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.ACCOUNTS);

	[RelayCommand]
	public async Task AuthorsExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.AUTHORS);

	[RelayCommand]
	public async Task ProductsExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.PRODUCTS);

	[RelayCommand]
	public async Task SearchExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.PRODUCT_SEARCH);

	[RelayCommand]
	public async Task TransactionsExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.TRANSACTIONS);

	[RelayCommand]
	public async Task ETHTransferExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.TRANSFER);
	
	[RelayCommand]
	public async Task NetworkExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.NETWORK);
	
	[RelayCommand]
	public async Task SettingsExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.SETTINGS);
	
	[RelayCommand]
	public async Task HelpExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.HELP);
	
	[RelayCommand]
	public async Task AboutExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.ABOUT);
	
	[RelayCommand]
	public async Task WhatsNewExcuteAsync() => await Navigation.GoToUpwardsAsync(Routes.WHATS_NEW);

	internal async Task InitializeAsync()
	{
		List<AccountModel> accounts = await accountsService.ListAccountsAsync();
		ObservableCollection<DomainModel> domains = await domainsService.GetAccountDomainsAsync();
		ObservableCollection<ResourceModel> products = await resourcesService.GetAllProductsAsync();
		ObservableCollection<TransactionModel> transactions = await transactionsService.ListTransactionsAsync(null, null, 3);

		Accounts = new(accounts.Count < 3 ? accounts : accounts.Take(3));
		Domains = new(domains);
		Transactions = new(transactions);
		Products = new(products);
		NumberOfAccounts = accounts.Count;
	}
}
