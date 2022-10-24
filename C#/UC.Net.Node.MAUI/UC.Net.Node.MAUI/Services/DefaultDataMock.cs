namespace UC.Net.Node.MAUI.Services;

internal static class DefaultDataMock
{
	static string _defaultNotificationBody = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network";
	static string _defaultNotificationTitle = "Today at 16:00";
	static string _defaultAuthorDue = "Active due: 07/07/2022 (in 182 days)";

	public static Account Account1 => new("0x2AdA01E7fEe46327f7E18a439e87060645af6da3")
	{
		Name = "Primary Ultranet Account",
		Balance = 102.5124M,
		Color = ColorHelper.CreateRandomGradientColor(),
		Authors = new List<Author>
		{
			Author1,
		},
		Transactions = new List<Transaction>
		{
			CreateTransaction(),
			CreateTransaction(),
		},
	};

	public static Account Account2 => new("0x8Ba6145c7900B0830AC15fcC0487a7CFf7a3136f")
	{
		Name = "Main Ultranet Account",
		Balance = 5005.0094M,
		Color = ColorHelper.CreateRandomGradientColor(),
		Authors = new List<Author>
		{
			Author1,
		},
		Transactions = new List<Transaction>
		{
			CreateTransaction(),
			CreateTransaction(),
		},
	};

    public static Author Author1 => new()
	{
		BidStatus = BidStatus.None,
		Name = "amazon.com",
		ActiveDue = _defaultAuthorDue
	};

	public static CustomCollection<string> DefaultFilter = new() { "All", "To be expired", "Expired", "Hidden", "Shown" };
	public static CustomCollection<string> ProductsFilter = new() { "Recent", "By author" };

	public static CustomCollection<string> MonthList1 = new() { "April", "May", "June", "July", "Augest", "Spetemper", "November" };

	public static AccountColor CreateColor(string argb, Color border = null) => new() { Color = Color.FromArgb(argb), BorderColor = border ?? Colors.Transparent };

	public static Notification CreateNotification(Severity severity, NotificationType type) => new()
	{
        Title = _defaultNotificationTitle,
        Body = _defaultNotificationBody,
        Type = type,
        Severity = severity
    };

	public static Transaction CreateTransaction(
		TransactionStatus status = TransactionStatus.None,
		int unt = 0, string name = null) => new()
	{
		Id = Guid.NewGuid(),
		FromId = Common.GenerateUniqueID(6),
		ToId = Common.GenerateUniqueID(6),
		Status = status,
		Unt = unt,
		Name = name
	};
}
