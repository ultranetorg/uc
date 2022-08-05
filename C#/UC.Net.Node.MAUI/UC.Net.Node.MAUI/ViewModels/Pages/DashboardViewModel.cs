namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class DashboardViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();

	[ObservableProperty]
    private CustomCollection<Transaction> _transactions = new();

    public DashboardViewModel(ILogger<DashboardViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	[RelayCommand]
    public async void AuthorsExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new AuthorsPage());
    }

	[RelayCommand]
    public async void ProductsExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new ProductsPage());
    }
	
	[RelayCommand]
    public async void ETHTransferExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new ETHTransferPage());
    }
	
	[RelayCommand]
    public async void TransactionsCommandExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    public async void AccountsCommandExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new ManageAccountsPage());
    }

	#region Fake Data

	private void FillFakeData()
	{
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "47F0",
			Name = "Main ultranet"
		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "2T52",
			Name = "Main ultranet"
		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "9MDL",
			Name = "Main ultranet"
		});

		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 540,
			Name = "UNT Transfer",
			Status = TransactionsStatus.Pending
		});
		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "UNT Transfer",
			Status = TransactionsStatus.Sent
		});
		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "UNT Transfer",
			Status = TransactionsStatus.Failed
		});
	}

	#endregion Fake Data
}
