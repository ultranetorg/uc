namespace UC.Net.Node.MAUI.Services;

internal static class DefaultDataMock
{
	public static Wallet Wallet1 => new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };

	public static Wallet Wallet2 => new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47FO",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#56d7de"),
    };

	public static Wallet Wallet3 => new()
	{
		Id = Guid.NewGuid(),
		Unts = 5005,
		IconCode = "2T52",
		Name = "Main ultranet wallet",
		AccountColor = Color.FromArgb("#bb50dd"),
	};

    public static Author Author1 => new()
	{
		BidStatus = BidStatus.None,
		Name = "amazon.com",
		ActiveDue = "Active due: 07/07/2022 (in 182 days)"
	};

	public static CustomCollection<string> ProductsFilter1 = new() { "All", "To be expired", "Expired", "Hidden", "Shown" };
	public static CustomCollection<string> ProductsFilter2 = new() { "Recent", "By author" };

	public static CustomCollection<string> MonthList1 = new() { "April", "May", "June", "July", "Augest", "Spetemper", "November" };

	public static AccountColor CreateColor(string argb, Color border = null) => new() { Color = Color.FromArgb(argb), BorderColor = border ?? Colors.Transparent };

	public static Notification CreateNotification(Severity severity, NotificationType type) => new()
	{
        Title = "Today at 16:00",
        Body = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
        Type = type,
        Severity = severity
    };

	public static Transaction CreateTransaction(
		TransactionStatus status = TransactionStatus.None,
		int unt = 0, string name = null) => new()
	{
		Id = Guid.NewGuid(),
		FromId = Generator.GenerateUniqueID(6),
		ToId = Generator.GenerateUniqueID(6),
		Status = status,
		Unt = unt,
		Name = name
	};
}
