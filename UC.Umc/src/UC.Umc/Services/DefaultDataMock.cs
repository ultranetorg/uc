using System.Collections.ObjectModel;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Models.Common;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace UC.Umc.Services;

internal static class DefaultDataMock
{
	private static readonly string _defaultNotificationBody = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network";
	private static readonly string _defaultNotificationTitle = "Today at 16:00";
	private static readonly AccountModel _allAccountsOption = new(string.Empty)
	{
		Name = "All Accounts",
		Color = ColorHelper.CreateGradientColor(Colors.DarkGrey),
		Balance = 0,
		ShowAmount = false
	};

	//public static AccountModel CreateAccount(string name = "Main Ultranet Account", decimal balance = 100M)
	//{
	//	var settings = new Settings();
	//	var log = new Log();
	//	var vault = new Vault(settings, log);
	//	var round = new Roundchain(settings, log, null, vault, null);
	//	var acc = new Account(Nethereum.Signer.EthECKey.GenerateKey());
	//	var entry = new AccountEntry(round, acc);
	//	entry.Balance = new Coin(balance);
	//	return new AccountModel(entry)
	//	{
	//		Name = name, // "Primary Ultranet Account"
	//		Color = ColorHelper.CreateRandomGradientColor(),
	//	};
	//}

	public static AccountModel CreateAccount(
		string name = "Main Ultranet Account", decimal balance = 100M) =>
		new($"0x{CommonHelper.GenerateUniqueId(42)}")
		{
			Name = name,
			Color = ColorHelper.CreateRandomGradientColor(),
			Balance = balance,
		};

	public static AccountModel AllAccountOption => _allAccountsOption;

	public static DomainModel Author1 => new()
	{
		BidStatus = BidStatus.None,
		Name = "amazon.com",
		ExpirationDate = new DateTime(2022, 7, 7)
	};

	public static ObservableCollection<string> AuthorsFilter = new() { "All", "Owned", "Auction", "Watched", "Hidden", "Free", "Reserved" }; //"Shown", "Outdated"
	public static ObservableCollection<string> ProductsFilter = new() { "Name", "Version", "Author", "Recent" };

	public static ObservableCollection<string> MonthList1 = new() { "April", "May", "June", "July", "Augest", "Spetemper", "November" };

	public static NetworkInfo NetworkInfo = new()
	{
		NodesCount = 381,
		ActiveUsers = 94,
		Bandwidth = "180 TH/s",
		LastBlockDate = DateTime.UtcNow,
		RoundNumber = "36017"
	};

	public static AccountColor CreateColor(string argb, Color border = null) => new() { Color = Color.FromArgb(argb), BorderColor = border ?? Colors.Transparent };

	public static AccountColor CreateRandomColor() => CreateColor(ColorHelper.GetRandomColor().ToArgbHex());

	public static Notification CreateNotification(NotificationSeverity severity, NotificationType type) => new()
	{
		Title = _defaultNotificationTitle,
		Body = _defaultNotificationBody,
		Type = type,
		Severity = severity
	};

	public static TransactionModel CreateTransaction(
		AccountModel account,
		TransactionStatus status = TransactionStatus.None,
		int unt = 0, string name = null, DateTime? date = null) => new()
	{
		Hash = $"0x{CommonHelper.GenerateUniqueId(42)}",
		Account = account,
		FromId = CommonHelper.GenerateUniqueId(6),
		ToId = CommonHelper.GenerateUniqueId(6),
		Status = status,
		Unt = unt,
		Name = name,
		Date = date ?? DateTime.UtcNow,
		Size = "789B",
		Confirmations = 0
	};

	public static List<string> AddedList = new()
	{
		"The term was coined by Antoine Destutt de Tracy",
		"And philosopher, who conceived it in 1796",
		"To develop a rational system of ideas to oppose the"
	};

	public static List<string> FixedList = new()
	{
		"The sensations that people experience as they interact with the material world",
		"Cted to the terroristic phase of the revolution",
		"Extending the vocabulary beyond what the general reader already possessed"
	};
}
