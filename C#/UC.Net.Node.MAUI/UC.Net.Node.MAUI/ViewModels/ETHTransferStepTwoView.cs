using UC.Net.Node.MAUI.Controls;

namespace UC.Net.Node.MAUI.ViewModels;
public partial class ETHTransferStepTwoViewModel : BaseViewModel
{
	public ETHTransferStepTwoViewModel(ILogger<ETHTransferStepTwoViewModel> logger): base(logger)
	{
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "47F0",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#6601e3"),
		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "2T52", 
			Name = "Primary ultranet wallet",
			AccountColor = Color.FromArgb("#3765f4"),

		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "9MDL",
			Name = "Secondary wallet",
			AccountColor = Color.FromArgb("#4cb16c"),

		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "UYO3",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#e65c93"),

		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "47FO",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#ba918c"),

		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "2T52",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#d56a48"),

		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "47FO",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#56d7de"),

		});
		Wallets.Add(new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "2T52",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#bb50dd"),

		});
	}

	CustomCollection<Wallet> _Wallets = new CustomCollection<Wallet>();
	public CustomCollection<Wallet> Wallets
	{
		get { return _Wallets; }
		set { SetProperty(ref _Wallets, value); }
	}

	public Page Page { get; }
	AccountColor _SelectedAccountColor;
	public AccountColor SelectedAccountColor
	{
		get { return _SelectedAccountColor; }
		set { SetProperty(ref _SelectedAccountColor, value); }
	}
	public Command ItemTppedCommand
	{
		get => new Command<Wallet>(ItemTpped);
	}
	private void ItemTpped(Wallet wallet)
	{
		foreach (var item in Wallets)
		{
			item.IsSelected = false;
		}
		wallet.IsSelected = true;
	}
	public Command ShowHideAccountsCommand
	{
		get => new Command(ShowHideAccounts);
	}
	private void ShowHideAccounts()
	{
		AccountsShown = !AccountsShown;
	}
	bool _ShowHideAccounts;
	public bool AccountsShown
	{
		get { return _ShowHideAccounts; }
		set { SetProperty(ref _ShowHideAccounts, value); }
	}
	Wallet _wallet = new Wallet
	{
		Id = Guid.NewGuid(),
		Unts = 5005,
		IconCode = "47F0",
		Name = "Main ultranet wallet",
		AccountColor = Color.FromArgb("#6601e3"),
	};
	public Wallet Wallet
	{
		get { return _wallet; }
		set { SetProperty(ref _wallet, value); }
	}
}
