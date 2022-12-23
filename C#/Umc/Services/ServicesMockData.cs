namespace UC.Umc.Services;

public class ServicesMockData : IServicesMockData
{
	private readonly ILogger<ServicesMockData> _logger;

	public IList<AccountViewModel> Accounts { get; private set; } = new List<AccountViewModel>();
    public IList<AuthorViewModel> Authors { get; private set; } = new List<AuthorViewModel>();
    public IList<ProductViewModel> Products { get; private set; } = new List<ProductViewModel>();
    public IList<TransactionViewModel> Transactions { get; private set; } = new List<TransactionViewModel>();
	public IList<AccountColor> AccountColors { get; private set; } = new List<AccountColor>();
    public IList<Emission> Emissions { get; private set; } = new List<Emission>();
    public IList<Notification> Notifications { get; private set; } = new List<Notification>();

    public ServicesMockData(ILogger<ServicesMockData> logger)
	{
		_logger = logger;
		InitializeMockData();
    }

	private void InitializeMockData()
	{
		try
		{
			#region Products

			ProductViewModel product1 = new("Windows", "Microsoft");
			ProductViewModel product2 = new("Office", "Microsoft");
			ProductViewModel product3 = new("Visual Studio Code", "Microsoft");
			ProductViewModel product4 = new("Outlook 365", "Microsoft");
			ProductViewModel product5 = new("Paint", "Microsoft");
			ProductViewModel product6 = new("Google Search", "Alphabet");
			ProductViewModel product7 = new("AWS", "Amazon");
			ProductViewModel product8 = new("Warehouse", "Space X");
			ProductViewModel product9 = new("Gate Defender 3", "Gate 500");

			Products = new List<ProductViewModel>()
			{
				product1,
				product2,
				product3,
				product4,
				product5,
				product6,
				product7,
				product8,
				product9,
			};

			#endregion Products

			#region Authors

			AuthorViewModel author1 = new()
			{
				Name = "microsoft",
				Title = "Microsoft",
				ActiveDue = "01/02/2023",
				BidStatus = BidStatus.Higher,
				Products = new List<ProductViewModel>
				{
					product1,
					product2,
					product3,
					product4,
				},
			};
			AuthorViewModel author2 = new()
			{
				Name = "alphabet",
				Title = "Alphabet",
				ActiveDue = "01/03/2023",
				BidStatus = BidStatus.Lower,
				Products = new List<ProductViewModel>
				{
					product6,
				},
			};
			AuthorViewModel author3 = new()
			{
				Name = "amazonlimited",
				Title = "Amazon Limited",
				ActiveDue = "01/03/2024",
				BidStatus = BidStatus.Higher,
				Products = new List<ProductViewModel>
				{
					product7,
					product8,
				},
			};
			AuthorViewModel author4 = new()
			{
				Name = "spacex",
				Title = "Space X",
				ActiveDue = "01/09/2023",
				BidStatus = BidStatus.Lower,
			};

			AuthorViewModel author5 = new()
			{
				Name = "gate500",
				Title = "Gate 500",
				ActiveDue = "01/09/2023",
				BidStatus = BidStatus.None,
				Products = new List<ProductViewModel>
				{
					product9,
				},
			};

			AuthorViewModel author6 = new()
			{
				Name = "Test",
				Title = "Test",
				ActiveDue = "01/09/2023",
				BidStatus = BidStatus.None,
				Products = new List<ProductViewModel>
				{
					product5
				},
			};

			product1.Author = author1;
			product2.Author = author1;
			product3.Author = author1;
			product4.Author = author1;
			product5.Author = author2;
			product6.Author = author2;
			product7.Author = author3;
			product8.Author = author4;
			product9.Author = author5;

			Authors.Add(author1);
			Authors.Add(author2);
			Authors.Add(author3);
			Authors.Add(author4);
			Authors.Add(author5);

			#endregion Authors

			#region Accounts
			
			TransactionViewModel transaction1 = DefaultDataMock.CreateTransaction(TransactionStatus.Pending, 50, "UNT Transfer");
			TransactionViewModel transaction2 = DefaultDataMock.CreateTransaction(TransactionStatus.Received, 100, "UNT Transfer");
			TransactionViewModel transaction3 = DefaultDataMock.CreateTransaction(TransactionStatus.Failed, 234, "UNT Transfer");
			TransactionViewModel transaction4 = DefaultDataMock.CreateTransaction(TransactionStatus.Sent, 10, "UNT Transfer");
			TransactionViewModel transaction5 = DefaultDataMock.CreateTransaction(TransactionStatus.None, 5290, "UNT Transfer");
			TransactionViewModel transaction6 = DefaultDataMock.CreateTransaction();
			TransactionViewModel transaction7 = DefaultDataMock.CreateTransaction();
			TransactionViewModel transaction8 = DefaultDataMock.CreateTransaction(TransactionStatus.Sent);
			TransactionViewModel transaction9 = DefaultDataMock.CreateTransaction(TransactionStatus.Received);
			TransactionViewModel transaction10 = DefaultDataMock.CreateTransaction(TransactionStatus.Failed);
			TransactionViewModel transaction11 = DefaultDataMock.CreateTransaction(TransactionStatus.Received, 50);
			TransactionViewModel transaction12 = DefaultDataMock.CreateTransaction(TransactionStatus.Received, 100);
			TransactionViewModel transaction13 = DefaultDataMock.CreateTransaction(TransactionStatus.Received, 500);
			TransactionViewModel transaction14 = DefaultDataMock.CreateTransaction(TransactionStatus.Pending, 200);
			TransactionViewModel transaction15 = DefaultDataMock.CreateTransaction(TransactionStatus.Pending, 1100);
			TransactionViewModel transaction16 = DefaultDataMock.CreateTransaction(TransactionStatus.Pending, 1100, "Transfer");
			TransactionViewModel transaction17 = DefaultDataMock.CreateTransaction();
			TransactionViewModel transaction18 = DefaultDataMock.CreateTransaction();

			Transactions.Add(transaction1);
			Transactions.Add(transaction2);
			Transactions.Add(transaction3);
			Transactions.Add(transaction4);
			Transactions.Add(transaction5);
			Transactions.Add(transaction6);
			Transactions.Add(transaction7);

			AccountViewModel account1 = DefaultDataMock.CreateAccount("Main Ultranet Account", 5005.0094M);
			AccountViewModel account2 = DefaultDataMock.CreateAccount("Primary Ultranet Account", 102.5124M);
			AccountViewModel account3 = DefaultDataMock.CreateAccount("Secondary Ultranet Account", 2.982258m);
			AccountViewModel account4 = DefaultDataMock.CreateAccount("Account for Payments", 65.61161m);
			AccountViewModel account5 = DefaultDataMock.CreateAccount("Accounts for Fees", 0.94707m);
			AccountViewModel account6 = DefaultDataMock.CreateAccount("Money for bet", 84.4341m);
			AccountViewModel account7 = DefaultDataMock.CreateAccount("Test account 1", 0);
			AccountViewModel account8 = DefaultDataMock.CreateAccount("Test account 2", 10M);
			AccountViewModel account9 = DefaultDataMock.CreateAccount("Test account 3", 0);
			AccountViewModel account10 = DefaultDataMock.CreateAccount("Test account 4", 120M);
			AccountViewModel account11 = DefaultDataMock.CreateAccount("Test account 5");

			author1.Account = account1;
			author2.Account = account1;
			transaction1.Account = account1;
			transaction2.Account = account1;
			transaction3.Account = account1;
			transaction4.Account = account1;
			transaction5.Account = account1;

			author3.Account = account2;
			transaction6.Account = account2;
			transaction7.Account = account2;

			transaction6.Account = account3;
			transaction7.Account = account3;

			transaction8.Account = account4;
			transaction9.Account = account4;

			author4.Account = account5;
			transaction10.Account = account5;
			transaction11.Account = account5;
			transaction12.Account = account5;
			transaction13.Account = account5;

			author5.Account = account6;
			transaction14.Account = account6;
			transaction15.Account = account6;
			transaction16.Account = account6;
			transaction17.Account = account6;
			transaction18.Account = account6;

			Accounts = new List<AccountViewModel>
			{
				account1,
				account2,
				account3,
				account4,
				account5,
				account6,
				account7,
				account8,
				account9,
				account10,
				account11,
			};

			#endregion Accounts
		
			AccountColors.Add(DefaultDataMock.CreateColor("#6601e3", Shell.Current.BackgroundColor));
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());

			Emissions.Add(new Emission { ETH = "100", Number = 1, UNT = "100" });
			Emissions.Add(new Emission { ETH = "1000", Number = 2, UNT = "1000" });
			Emissions.Add(new Emission { ETH = "10000", Number = 3, UNT = "10000" });
			Emissions.Add(new Emission { ETH = "100000", Number = 4, UNT = "10000" });

			Notifications.Add(DefaultDataMock.CreateNotification(Severity.High, NotificationType.ProductOperations));
			Notifications.Add(DefaultDataMock.CreateNotification(Severity.Low, NotificationType.SystemEvent));
			Notifications.Add(DefaultDataMock.CreateNotification(Severity.Mid, NotificationType.AuthorOperations));
			Notifications.Add(DefaultDataMock.CreateNotification(Severity.High, NotificationType.TokenOperations));
			Notifications.Add(DefaultDataMock.CreateNotification(Severity.Low, NotificationType.Server));
			Notifications.Add(DefaultDataMock.CreateNotification(Severity.Mid, NotificationType.Wallet));
			Notifications.Add(DefaultDataMock.CreateNotification(Severity.High, NotificationType.Server));
		}
		catch(Exception ex)
		{
            _logger.LogError(ex, "InitializeMockData Exception: {Ex}", ex.Message);
		}
	}
}
