namespace UC.Net.Node.MAUI.Services;

public class ServicesMockData : IServicesMockData
{
    public IList<Account> Accounts { get; set; } = new List<Account>();
    public IList<Author> Authors => Accounts.SelectMany(x => x.Authors).ToList();
    public IList<Product> Products => Authors.SelectMany(x => x.Products).ToList();
    public IEnumerable<Transaction> Transactions => Accounts.SelectMany(x => x.Transactions);
    public IList<Wallet> Wallets => Accounts.SelectMany(x => x.Wallets).ToList();
	public IList<AccountColor> AccountColors { get; set; } = new List<AccountColor>();
    public IList<Emission> Emissions { get; set; } = new List<Emission>();
    public IList<Notification> Notifications { get; set; } = new List<Notification>();

    public ServicesMockData()
	{
		InitializeMockData();
    }

	private void InitializeMockData()
	{
		try
		{
			#region Products

			Product product1 = new("Windows", "Microsoft");
			Product product2 = new("Office", "Microsoft");
			Product product3 = new("Visual Studio Code", "Microsoft");
			Product product4 = new("Outlook 365", "Microsoft");
			Product product5 = new("Paint", "Microsoft");
			Product product6 = new("Google Search", "Alphabet");
			Product product7 = new("AWS", "Amazon");
			Product product8 = new("Warehouse", "Space X");
			Product product9 = new("Gate Defender 3", "Gate 500");

			#endregion Products

			#region Authors

			Author author1 = new()
			{
				Name = "microsoft",
				Title = "Microsoft",
				ActiveDue = "01/02/2023",
				BidStatus = BidStatus.Higher,
				Products = new List<Product>
				{
					product1,
					product2,
					product3,
					product4,
					product5,
				},
			};
			product1.Author = author1;
			product2.Author = author1;
			product3.Author = author1;
			product4.Author = author1;
			product5.Author = author1;

			Author author2 = new()
			{
				Name = "alphabet",
				Title = "Alphabet",
				ActiveDue = "01/03/2023",
				BidStatus = BidStatus.Lower,
				Products = new List<Product>
				{
					product6,
				},
			};
			product6.Author = author2;

			Author author3 = new()
			{
				Name = "amazonlimited",
				Title = "Amazon Limited",
				ActiveDue = "01/03/2024",
				BidStatus = BidStatus.Higher,
				Products = new List<Product>
				{
					product7,
					product8,
				},
			};
			product7.Author = author3;
			product8.Author = author3;

			Author author4 = new()
			{
				Name = "spacex",
				Title = "Space X",
				ActiveDue = "01/09/2023",
				BidStatus = BidStatus.Lower,
			};

			Author author5 = new()
			{
				Name = "gate500",
				Title = "Gate 500",
				ActiveDue = "01/09/2023",
				BidStatus = BidStatus.Lower,
				Products = new List<Product>
				{
					product9,
				},
			};
			product9.Author = author5;

			#endregion Authors

			#region Accounts
			
			Transaction transaction1 = DefaultDataMock.CreateTransaction(TransactionStatus.Pending, 50, "UNT Transfer");
			Transaction transaction2 = DefaultDataMock.CreateTransaction(TransactionStatus.Received, 100, "UNT Transfer");
			Transaction transaction3 = DefaultDataMock.CreateTransaction(TransactionStatus.Failed, 234, "UNT Transfer");
			Transaction transaction4 = DefaultDataMock.CreateTransaction(TransactionStatus.Sent, 10, "UNT Transfer");
			Transaction transaction5 = DefaultDataMock.CreateTransaction(TransactionStatus.None, 5290, "UNT Transfer");
			Transaction transaction6 = DefaultDataMock.CreateTransaction();
			Transaction transaction7 = DefaultDataMock.CreateTransaction();

			Account account1 = new("0x8Ba6145c7900B0830AC15fcC0487a7CFf7a3136f")
			{
				Name = "Main Ultranet Account",
				Balance = 5005.0094M,
				Color = GradientColor.FromColors(Color.FromRgb(0, 0, 15), Color.FromRgb(0, 0, 115)),
				Authors = new List<Author>
				{
					author1,
					author2,
				},
				Transactions = new List<Transaction>
				{
					transaction1,
					transaction2,
					transaction3,
					transaction4,
					transaction5,
					transaction6,
					transaction7,
				},
			};
			author1.Account = account1;
			author2.Account = account1;
			transaction1.Account = account1;
			transaction2.Account = account1;
			transaction3.Account = account1;
			transaction6.Account = account1;
			transaction7.Account = account1;

			Account account2 = new("0x2AdA01E7fEe46327f7E18a439e87060645af6da3")
			{
				Name = "Primary Ultranet Account",
				Balance = 102.5124M,
				Authors = new List<Author>
				{
					author3,
				},
				Transactions = new List<Transaction>
				{
					transaction4,
					transaction5,
				},
			};
			author3.Account = account2;
			transaction4.Account = account1;
			transaction5.Account = account1;

			Account account3 = new("0x47C0566c10c00cfc29aa439Fa1C7d4B888b413D0")
			{
				Name = "Secondary Ultranet Account",
				Balance = 2.982258M,
				Transactions = new List<Transaction>
				{
					transaction6,
					transaction7,
				},
			};
			transaction6.Account = account1;
			transaction7.Account = account1;

			Transaction transaction8 = DefaultDataMock.CreateTransaction(TransactionStatus.Sent);
			Transaction transaction9 = DefaultDataMock.CreateTransaction(TransactionStatus.Received);

			Account account4 = new("0x0362C7DfE8B4A40D3374D5e22cDF102c33E44df0")
			{
				Name = "Account for Payments",
				Balance = 65.61161M,
				Transactions = new List<Transaction>
				{
					transaction8,
					transaction9,
				},
			};
			transaction8.Account = account4;
			transaction9.Account = account4;

			Transaction transaction10 = DefaultDataMock.CreateTransaction(TransactionStatus.Failed);
			Transaction transaction11 = DefaultDataMock.CreateTransaction(TransactionStatus.Received, 50);
			Transaction transaction12 = DefaultDataMock.CreateTransaction(TransactionStatus.Received, 100);
			Transaction transaction13 = DefaultDataMock.CreateTransaction(TransactionStatus.Received, 500);

			Account account5 = new("0x033A45c42b1E4C6beEdF0dc76e847A942B37e0Ba")
			{
				Name = "Accounts for Fees",
				Balance = 0.94707M,
				Authors = new List<Author>
				{
					author4,
				},
				Transactions = new List<Transaction>
				{
					transaction10,
					transaction11,
					transaction12,
					transaction13,
				},
			};
			author4.Account = account5;
			transaction10.Account = account5;
			transaction11.Account = account5;
			transaction12.Account = account5;
			transaction13.Account = account5;

			Transaction transaction14 = DefaultDataMock.CreateTransaction(TransactionStatus.Pending, 200);
			Transaction transaction15 = DefaultDataMock.CreateTransaction(TransactionStatus.Pending, 1100);
			Transaction transaction16 = DefaultDataMock.CreateTransaction(TransactionStatus.Pending, 1100, "Transfer");
			Transaction transaction17 = DefaultDataMock.CreateTransaction();
			Transaction transaction18 = DefaultDataMock.CreateTransaction();

			Account account6 = new("0x59E58E6821BC870fB2E125477d58b50F23a8De5c")
			{
				Name = "Money for bet",
				Balance = 84.4341M,
				Authors = new List<Author>
				{
					author5,
				},
				Transactions = new List<Transaction>
				{
					transaction14,
					transaction15,
					transaction16,
					transaction17,
					transaction18,
				},
			};
			author5.Account = account6;
			transaction14.Account = account6;
			transaction15.Account = account6;
			transaction16.Account = account6;
			transaction17.Account = account6;
			transaction18.Account = account6;

			Account account7 = new("0xD786C52802740f16Ca920876F827AAe62D54F4a0")
			{
				Name = "Test account 1",
				Balance = 0,
			};

			Account account8 = new("0xD90144134D9165AE356F603331655cD95e662250")
			{
				Name = "Test account 2",
				Balance = 0,
			};

			Account account9 = new("0x556c92A73DBDC40aF2D82Eb3Bf7CCed5eEDd8Ae3")
			{
				Name = "Test account 3",
				Balance = 0,
			};

			Account account10 = new("0xBDfc21E1812E9eF07f6bE562ded4Af59F0e065a1")
			{
				Name = "Test account 4",
				Balance = 0,
			};

			Account account11 = new("0xE3b7e424C2335944991a1aec6317008A138252cb")
			{
				Name = "Test account 5",
				Balance = 0,
			};

			var wallet1 = DefaultDataMock.Wallet1;
			var wallet2 = DefaultDataMock.Wallet2;
			var wallet3 = DefaultDataMock.Wallet3;

			account1.Wallets = new List<Wallet> { wallet1, wallet2, wallet3 };
			account3.Wallets = new List<Wallet> { wallet1, wallet2 };
			account3.Wallets = new List<Wallet> { wallet1, wallet3 };
			account2.Wallets = new List<Wallet> { wallet2, wallet3 };

			Accounts = new List<Account>
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
			AccountColors.Add(DefaultDataMock.CreateColor("#3765f4"));
			AccountColors.Add(DefaultDataMock.CreateColor("#ba918c"));
			AccountColors.Add(DefaultDataMock.CreateColor("#d56a48"));
			AccountColors.Add(DefaultDataMock.CreateColor("#56d7de"));
			AccountColors.Add(DefaultDataMock.CreateColor("#bb50dd"));

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
			// TBD
		}
	}
}
