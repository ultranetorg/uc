namespace UC.Umc.Services;

internal static class DefaultDataMock
{
	static string _defaultNotificationBody = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network";
	static string _defaultNotificationTitle = "Today at 16:00";
	static string _defaultAuthorDue = "Active due: 07/07/2022 (in 182 days)";

	//public static AccountViewModel CreateAccount(string name = "Main Ultranet Account", decimal balance = 100M) 
	//{
	//	var settings = new Settings();
	//	var log = new Log();
	//	var vault = new Vault(settings, log);
	//	var round = new Roundchain(settings, log, null, vault, null);
	//	var acc = new Account(Nethereum.Signer.EthECKey.GenerateKey());
	//	var entry = new AccountEntry(round, acc);
	//	entry.Balance = new Coin(balance);
	//	return new AccountViewModel(entry)
	//	{
	//		Name = name, // "Primary Ultranet Account"
	//		Color = ColorHelper.CreateRandomGradientColor(),
	//	};
	//}

	public static AccountViewModel CreateAccount(string name = "Main Ultranet Account", decimal balance = 100M)
	{
		return new AccountViewModel("0x72329af958b5679e0354ff12fb27ddbf34d37aca")
		{
			Name = name, // "Primary Ultranet Account"
			Color = ColorHelper.CreateRandomGradientColor(),
			Balance = balance,
		};
	}

	public static AuthorViewModel Author1 => new()
	{
		BidStatus = BidStatus.None,
		Name = "amazon.com",
		ActiveDue = _defaultAuthorDue
	};

	public static CustomCollection<string> AuthorsFilter = new() { "All", "My Own", "Auction", "Watched", "Hidden", "Shown", "Outdated" };
	public static CustomCollection<string> ProductsFilter = new() { "Recent", "By author" };

	public static CustomCollection<string> MonthList1 = new() { "April", "May", "June", "July", "Augest", "Spetemper", "November" };

	public static AccountColor CreateColor(string argb, Color border = null) => new() { Color = Color.FromArgb(argb), BorderColor = border ?? Colors.Transparent };

	public static AccountColor CreateRandomColor() => CreateColor(ColorHelper.GetRandomColor().ToArgbHex());

	public static Notification CreateNotification(Severity severity, NotificationType type) => new()
	{
        Title = _defaultNotificationTitle,
        Body = _defaultNotificationBody,
        Type = type,
        Severity = severity
    };

	public static TransactionViewModel CreateTransaction(
		TransactionStatus status = TransactionStatus.None,
		int unt = 0, string name = null) => new()
	{
		Id = Guid.NewGuid(),
		FromId = CommonHelper.GenerateUniqueID(6),
		ToId = CommonHelper.GenerateUniqueID(6),
		Status = status,
		Unt = unt,
		Name = name
	};
}
