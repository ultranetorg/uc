namespace UC.Net.Node.MAUI.Services;

public class ServicesMockData : IServicesMockData
{
    public ServicesMockData()
	{
		#region Transaction

		Transaction transaction1 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Status = TransactionStatus.None,
		};
		Transaction transaction2 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 50,
			Name = "UNT Transfer",
			Status = TransactionStatus.Pending,
		};
		Transaction transaction3 = new()
		{
			Id = 1,
			Status = TransactionStatus.Sent,
		};
		Transaction transaction4 = new()
		{
			Id = 1,
			Status = TransactionStatus.Failed,
		};
		Transaction transaction5 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Status = TransactionStatus.Received,
		};
		Transaction transaction6 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};

		Transaction transaction7 = new()
		{
			Id = 1,
			Status = TransactionStatus.Pending,
		};
		Transaction transaction8 = new()
		{
			Id = 1,
			Status = TransactionStatus.Sent,
		};
		Transaction transaction9 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Status = TransactionStatus.Received,
		};
		Transaction transaction10 = new()
		{
			Id = 1,
			Status = TransactionStatus.Failed,
		};
		Transaction transaction11 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};

		Transaction transaction12 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 5290,
			Name = "UNT Transfer",
			Status = TransactionStatus.None,
		};

		Transaction transaction13 = new()
		{
			Id = 1,
			Status = TransactionStatus.Pending,
		};
		Transaction transaction14 = new()
		{
			Id = 1,
			Status = TransactionStatus.Sent,
		};
		Transaction transaction15 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Status = TransactionStatus.Received,
		};
		Transaction transaction16 = new()
		{
			Id = 1,
			Status = TransactionStatus.Failed,
		};
		Transaction transaction17 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};
		Transaction transaction18 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 190,
			Name = "UNT Transfer",
			Status = TransactionStatus.None,
		};
		Transaction transaction19 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};

		Transaction transaction20 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Status = TransactionStatus.Sent,
		};
		Transaction transaction21 = new()
		{
			Id = 1,
			Status = TransactionStatus.Failed,
		};
		Transaction transaction22 = new()
		{
			Id = 1,
			Status = TransactionStatus.Received,
		};
		Transaction transaction23 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};
		Transaction transaction24 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};
		Transaction transaction25 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};
		Transaction transaction26 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 520,
			Name = "UNT Transfer",
			Status = TransactionStatus.None,
		};
		Transaction transaction27 = new()
		{
			Id = 1,
			Status = TransactionStatus.Received,
		};
		Transaction transaction28 = new()
		{
			Id = 1,
			Status = TransactionStatus.Failed,
		};
		Transaction transaction29 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Status = TransactionStatus.Sent,
		};
		Transaction transaction30 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};
		Transaction transaction31 = new()
		{
			Id = 1,
			Status = TransactionStatus.None,
		};
		Transaction transaction32 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 540,
			Name = "UNT Transfer",
			Status = TransactionStatus.Failed,
		};
		Transaction transaction33 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "UNT Transfer",
			Status = TransactionStatus.Sent,
		};
		Transaction transaction34 = new()
		{
			Id = 1,
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "UNT Transfer",
			Status = TransactionStatus.Received,
		};

		#endregion Transaction

		#region Products

		Product product1 = new()
		{
			Name = "Windows",
		};
		Product product2 = new()
		{
			Name = "Office",
		};
		Product product3 = new()
		{
			Name = "Visual Studio Code",
		};
		Product product4 = new()
		{
			Name = "Outlook 365",
		};
		Product product5 = new()
		{
			Name = "Paint",
		};
		Product product6 = new()
		{
			Name = "Google Search",
		};
		Product product7 = new()
		{
			Name = "AWS",
		};
		Product product8 = new()
		{
			Name = "Warehouse",
		};
		Product product9 = new()
		{
			Name = "Gate Defender 3",
		};

		#endregion Products

		#region Authors

		Author author1 = new()
		{
			Name = "microsoft",
			Title = "Microsoft",
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
		};

		Author author5 = new()
		{
			Name = "gate500",
			Title = "Gate 500",
			Products = new List<Product>
			{
				product9,
			},
		};
		product9.Author = author5;

		#endregion Authors

		#region Accounts

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
				transaction8,
			},
		};
		author1.Account = account1;
		author2.Account = account1;
		transaction1.Account = account1;
		transaction2.Account = account1;
		transaction3.Account = account1;
		transaction4.Account = account1;
		transaction5.Account = account1;
		transaction6.Account = account1;
		transaction7.Account = account1;
		transaction8.Account = account1;

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
				transaction8,
				transaction9,
			},
		};
		author3.Account = account2;
		transaction8.Account = account2;
		transaction9.Account = account2;

		Account account3 = new("0x47C0566c10c00cfc29aa439Fa1C7d4B888b413D0")
		{
			Name = "Secondary Ultranet Account",
			Balance = 2.982258M,
			Transactions = new List<Transaction>
			{
				transaction10,
				transaction11,
				transaction12,
				transaction13,
				transaction14,
				transaction15,
				transaction16,
				transaction17,
			},
		};
		transaction10.Account = account3;
		transaction11.Account = account3;
		transaction12.Account = account3;
		transaction13.Account = account3;
		transaction14.Account = account3;
		transaction15.Account = account3;
		transaction16.Account = account3;
		transaction17.Account = account3;

		Account account4 = new("0x0362C7DfE8B4A40D3374D5e22cDF102c33E44df0")
		{
			Name = "Account for Payments",
			Balance = 65.61161M,
			Transactions = new List<Transaction>
			{
				transaction18,
				transaction19,
				transaction20,
			},
		};
		transaction18.Account = account4;
		transaction19.Account = account4;
		transaction20.Account = account4;

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
				transaction21,
				transaction22,
				transaction23,
				transaction24,
				transaction25,
			},
		};
		author4.Account = account5;
		transaction21.Account = account5;
		transaction22.Account = account5;
		transaction23.Account = account5;
		transaction24.Account = account5;
		transaction25.Account = account5;

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
				transaction26,
				transaction27,
				transaction28,
				transaction29,
				transaction30,
				transaction31,
				transaction32,
				transaction33,
				transaction34,
			},
		};
		author5.Account = account6;
		transaction26.Account = account6;
		transaction27.Account = account6;
		transaction28.Account = account6;
		transaction29.Account = account6;
		transaction30.Account = account6;
		transaction31.Account = account6;
		transaction32.Account = account6;
		transaction33.Account = account6;
		transaction34.Account = account6;

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

		var wallet1 = new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "47F0",
			Name = "Main ultranet",
			AccountColor = Color.FromArgb("#6601e3"),
		};
		var wallet2 = new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "2T52",
			Name = "Main ultranet"
		};
		var wallet3 = new Wallet
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "9MDL",
			Name = "Main ultranet"
		};

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

		AccountColors.Add(new AccountColor { Color = Color.FromArgb("#6601e3") ,BoderColor = Shell.Current.BackgroundColor });
        AccountColors.Add(new AccountColor { Color = Color.FromArgb("#3765f4"), BoderColor = Colors.Transparent });
        AccountColors.Add(new AccountColor { Color = Color.FromArgb("#4cb16c"),  BoderColor = Colors.Transparent });
        AccountColors.Add(new AccountColor { Color = Color.FromArgb("#ba918c"), BoderColor = Colors.Transparent });
        AccountColors.Add(new AccountColor { Color = Color.FromArgb("#d56a48"),  BoderColor = Colors.Transparent });
        AccountColors.Add(new AccountColor { Color = Color.FromArgb("#56d7de"), BoderColor = Colors.Transparent });
        AccountColors.Add(new AccountColor { Color = Color.FromArgb("#bb50dd"), BoderColor = Colors.Transparent });

        Emissions.Add(new Emission { ETH = "100", Number = 1,UNT = "100" });
        Emissions.Add(new Emission { ETH = "1000", Number = 2, UNT = "1000" });
        Emissions.Add(new Emission { ETH = "10000", Number = 3, UNT = "10000" });
        Emissions.Add(new Emission { ETH = "100000", Number = 4, UNT = "10000" });
    }

    public IList<Account> Accounts { get; set; }
    public IList<Author> Authors => Accounts.SelectMany(x => x.Authors).ToList();
    public IList<Product> Products => Authors.SelectMany(x => x.Products).ToList();
    public IEnumerable<Transaction> Transactions => Accounts.SelectMany(x => x.Transactions);
    public IList<Wallet> Wallets => Accounts.SelectMany(x => x.Wallets).ToList();
	public IList<AccountColor> AccountColors { get; set; }
    public IList<Emission> Emissions { get; set; }
}
