using UC.Umc.Common.Constants;
using UC.Umc.Data.Classes;

namespace UC.Umc.Services;

internal static class DefaultDataMock
{
	private static readonly string _defaultNotificationBody = Properties.Dashboard_Strings.Notification_Default1;
	private static readonly string _defaultNotificationTitle = Properties.Dashboard_Strings.Notification_Default2;

	private static readonly AccountViewModel _allAccountsOption = new(string.Empty)
	{
		Name = Properties.Account_Strings.All_Accounts,
		Color = ColorHelper.CreateGradientColor(Colors.DarkGrey),
		Balance = 0,
		ShowAmount = false
	};

	public static AccountViewModel CreateAccount(
		string name = null, decimal balance = 100M) =>
		new($"0x{CommonHelper.GenerateUniqueId(CommonConstants.LENGTH_HASH)}")
		{
			Name = name ?? Properties.Account_Strings.Main_Account,
			Color = ColorHelper.CreateRandomGradientColor(),
			Balance = balance,
		};

	public static AccountViewModel AllAccountOption => _allAccountsOption;

	public static AuthorViewModel Author1 => new()
	{
		BidStatus = BidStatus.None,
		Name = "amazon.com",
		ExpirationDate = new DateTime(2022, 7, 7)
	};

	public static CustomCollection<AuthorFilter> AuthorsFilter = new()
	{
		new AuthorFilter(Properties.Author_Strings.Filter_All),
		new AuthorFilter(Properties.Author_Strings.Filter_Auction),
		new AuthorFilter(Properties.Author_Strings.Filter_Free),
		new AuthorFilter(Properties.Author_Strings.Filter_Hidden),
		new AuthorFilter(Properties.Author_Strings.Filter_Owned),
		new AuthorFilter(Properties.Author_Strings.Filter_Reserved),
		new AuthorFilter(Properties.Author_Strings.Filter_Watched),
	}; // "Shown", "Outdated"

	public static CustomCollection<string> ProductsFilter = new()
	{
		Properties.Product_Strings.Filter_Name,
		Properties.Product_Strings.Filter_Version,
		Properties.Product_Strings.Filter_Author,
		Properties.Product_Strings.Filter_Recent,
	};

	public static CustomCollection<string> MonthList1 = new()
	{
		Properties.Additional_Strings.Month_April,
		Properties.Additional_Strings.Month_May,
		Properties.Additional_Strings.Month_June,
		Properties.Additional_Strings.Month_July,
		Properties.Additional_Strings.Month_August,
		Properties.Additional_Strings.Month_September,
		Properties.Additional_Strings.Month_October,
		Properties.Additional_Strings.Month_November,
		Properties.Additional_Strings.Month_December,
		Properties.Additional_Strings.Month_January,
		Properties.Additional_Strings.Month_February,
		Properties.Additional_Strings.Month_March,
	};

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

	public static Notification CreateNotification(Severity severity, NotificationType type) => new()
	{
        Title = _defaultNotificationTitle,
        Body = _defaultNotificationBody,
        Type = type,
        Severity = severity
    };

	public static TransactionViewModel CreateTransaction(
		AccountViewModel account,
		TransactionStatus status = TransactionStatus.None,
		int unt = 0, string name = null, DateTime? date = null) => new()
	{
		Hash = $"0x{CommonHelper.GenerateUniqueId(CommonConstants.LENGTH_HASH)}",
		Account = account,
		FromId = CommonHelper.GenerateUniqueId(CommonConstants.LENGTH_TRANSACTION),
		ToId = CommonHelper.GenerateUniqueId(CommonConstants.LENGTH_TRANSACTION),
		Status = status,
		Unt = unt,
		Name = name,
		Date = date ?? DateTime.UtcNow,
		Size = "789B",
		Confirmations = 0
	};

	public static List<string> AddedList = new()
	{
		Properties.Additional_Strings.Version4_Update1,
		Properties.Additional_Strings.Version4_Update2,
		Properties.Additional_Strings.Version4_Update3,
	};

	public static List<string> FixedList = new()
	{
		Properties.Additional_Strings.Version4_Fix1,
		Properties.Additional_Strings.Version4_Fix2,
		Properties.Additional_Strings.Version4_Fix3,
	};
}
