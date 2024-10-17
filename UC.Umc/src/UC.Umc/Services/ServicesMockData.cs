using Microsoft.Extensions.Logging;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Models.Common;

namespace UC.Umc.Services;

public class ServicesMockData : IServicesMockData
{
	private readonly ILogger<ServicesMockData> _logger;

	public IList<AccountModel> Accounts { get; private set; } = new List<AccountModel>();
	public IList<DomainModel> Domains { get; private set; } = new List<DomainModel>();
	public IList<ResourceModel> Resources { get; private set; } = new List<ResourceModel>();
	public IList<TransactionModel> Transactions { get; private set; } = new List<TransactionModel>();
	public IList<AccountColor> AccountColors { get; private set; } = new List<AccountColor>();
	public IList<Emission> Emissions { get; private set; } = new List<Emission>();
	public IList<Notification> Notifications { get; private set; } = new List<Notification>();
	public IList<Bid> BidsHistory { get; private set; } = new List<Bid>();
	public IList<string> HelpQuestions  { get; private set; } = new List<string>();

	public ServicesMockData(ILogger<ServicesMockData> logger)
	{
		_logger = logger;
		InitializeMockData();
	}

	private void InitializeMockData()
	{
		try
		{
			#region Accounts

			AccountModel account1 = DefaultDataMock.CreateAccount("Main Ultranet Account", 5005.0094M);
			AccountModel account2 = DefaultDataMock.CreateAccount("Primary Ultranet Account", 102.5124M);
			AccountModel account3 = DefaultDataMock.CreateAccount("Secondary Ultranet Account", 2.982258m);
			AccountModel account4 = DefaultDataMock.CreateAccount("Account for Payments", 65.61161m);
			AccountModel account5 = DefaultDataMock.CreateAccount("Accounts for Fees", 0.94707m);
			AccountModel account6 = DefaultDataMock.CreateAccount("Money for bet", 84.4341m);
			AccountModel account7 = DefaultDataMock.CreateAccount("Test account 1", 0);
			AccountModel account8 = DefaultDataMock.CreateAccount("Test account 2", 10M);
			AccountModel account9 = DefaultDataMock.CreateAccount("Test account 3", 0);
			AccountModel account10 = DefaultDataMock.CreateAccount("Test account 4", 120M);
			AccountModel account11 = DefaultDataMock.CreateAccount("Test account 5");

			Accounts.Add(account1);
			Accounts.Add(account2);
			Accounts.Add(account3);
			Accounts.Add(account4);
			Accounts.Add(account5);
			Accounts.Add(account6);
			Accounts.Add(account7);
			Accounts.Add(account8);
			Accounts.Add(account9);
			Accounts.Add(account10);
			Accounts.Add(account11);

			#endregion Accounts

			#region Transactions

			Transactions.Add(DefaultDataMock.CreateTransaction(account10, TransactionStatus.None, 1100, "Test"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account11, TransactionStatus.None, 1100, "Test"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account3, TransactionStatus.Sent, 10, "UNT Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account3, TransactionStatus.Sent, 10, "UNT Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account4, TransactionStatus.Sent, 10, "UNT Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account4, TransactionStatus.Received, 10, "UNT Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account5, TransactionStatus.Sent, 10, "Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account5, TransactionStatus.Received, 50, "Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account6, TransactionStatus.Received, 100, "Transfer", new DateTime(2021, 10, 11)));
			Transactions.Add(DefaultDataMock.CreateTransaction(account6, TransactionStatus.Failed, 500, "Transfer", new DateTime(2020, 1, 5)));
			Transactions.Add(DefaultDataMock.CreateTransaction(account7, TransactionStatus.Pending, 200, "Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account8, TransactionStatus.Pending, 1100, "Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account9, TransactionStatus.Failed, 1100, "Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account1, TransactionStatus.Failed, 234, "UNT Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account2, TransactionStatus.Sent, 10, "UNT Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account2, TransactionStatus.Pending, 5290, "UNT Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account1, TransactionStatus.Pending, 50, "UNT Transfer"));
			Transactions.Add(DefaultDataMock.CreateTransaction(account1, TransactionStatus.Received, 100, "Ultranetorg author registration"));

			#endregion Transactions

			#region Resources

			ResourceModel product1 = new("Windows", "Microsoft", ColorHelper.GetRandomColor(), "1.0.1");
			ResourceModel product2 = new("Office", "Microsoft", ColorHelper.GetRandomColor(), "1.0.2");
			ResourceModel product3 = new("Visual Studio Code", "Microsoft", ColorHelper.GetRandomColor(), "1.0.3");
			ResourceModel product4 = new("Outlook 365", "Microsoft", ColorHelper.GetRandomColor(), "4.4 (LTS)");
			ResourceModel product5 = new("Paint", "Microsoft", ColorHelper.GetRandomColor(), "0.7-preview");
			ResourceModel product6 = new("Google Search", "Alphabet", ColorHelper.GetRandomColor(), "2.0.1");
			ResourceModel product7 = new("AWS", "Amazon", ColorHelper.GetRandomColor(), "12.2.0 beta");
			ResourceModel product8 = new("Warehouse", "Space X", ColorHelper.GetRandomColor(), "0.1-alpha");
			ResourceModel product9 = new("Gate Defender 3", "Gate 500", ColorHelper.GetRandomColor(), "1.0.9");

			Resources.Add(product1);
			Resources.Add(product2);
			Resources.Add(product3);
			Resources.Add(product4);
			Resources.Add(product5);
			Resources.Add(product6);
			Resources.Add(product7);
			Resources.Add(product8);
			Resources.Add(product9);

			#endregion Resources

			#region Bids History

			BidsHistory = new List<Bid>()
			{
				new()
				{
					Amount = 599,
					Date = new DateTime(2023,1,1),
					BidBy = "0x63FaC9201494f0bd17B9892B9f"
				},
				new()
				{
					Amount = 399,
					Date = new DateTime(2023,1,1),
					BidBy = "0x63FaC9201494f0bd17B9892B9f"
				},
				new()
				{
					Amount = 199,
					Date = new DateTime(2023,1,1),
					BidBy = "0x63FaC9201494f0bd17B9892B9f"
				},
				new()
				{
					Amount = 99,
					Date = new DateTime(2023,1,1),
					BidBy = "0x63FaC9201494f0bd17B9892B9f"
				},
				new()
				{
					Amount = 9,
					Date = new DateTime(2023,1,1),
					BidBy = "0x63FaC9201494f0bd17B9892B9f"
				}
			};

			#endregion Bids History

			#region Domains

			DomainModel author1 = new()
			{
				Name = "microsoft",
				Title = "Microsoft",
				AuctionEndDate = new DateTime(2024, 2, 1),
				Status = AuthorStatus.Auction,
				BidStatus = BidStatus.Higher,
				CurrentBid = 1000m,
				MaximumBidBy = "0x63FaC9201494f0bd17B9892B9f",
				Account = account1,
				Products = new List<ResourceModel>
				{
					product1,
					product2,
					product3,
					product4,
				},
			};
			DomainModel author2 = new()
			{
				Name = "alphabet",
				Title = "Alphabet",
				Account = account1,
				AuctionEndDate = new DateTime(2024, 3, 1),
				Status = AuthorStatus.Auction,
				BidStatus = BidStatus.Lower,
				CurrentBid = 1000m,
				MaximumBidBy = "0x63FaC9201494f0bd17B9892B9f",
				Products = new List<ResourceModel>
				{
					product6,
				},
			};
			DomainModel author3 = new()
			{
				Name = "amazonlimited",
				Title = "Amazon Limited",
				Account = account2,
				AuctionEndDate = new DateTime(2024, 3, 1),
				Status = AuthorStatus.Watched,
				CurrentBid = 9m,
				MaximumBidBy = "0x63FaC9201494f0bd17B9892B9f",
				Products = new List<ResourceModel>
				{
					product7,
					product8,
				},
			};
			DomainModel author4 = new()
			{
				Name = "spacex",
				Title = "Space X",
				Account = account5,
				Owner = "0x71C7656EC7ab88b098defB751B7401B5f6d76F",
				Status = AuthorStatus.Owned,
				ExpirationDate = new DateTime(2024, 9, 1),
			};

			DomainModel author5 = new()
			{
				Name = "gate500",
				Title = "Gate 500",
				Account = account6,
				Owner = "0x71C7656EC7ab88b098defB751B7401B5f6d76F",
				ExpirationDate = new DateTime(2024, 9, 1),
				Status = AuthorStatus.Reserved,
				Products = new List<ResourceModel>
				{
					product9,
				},
			};

			DomainModel author6 = new()
			{
				Name = "Test",
				Status = AuthorStatus.Free,
			};

			DomainModel author7 = new()
			{
				Name = "Test Own",
				Status = AuthorStatus.Owned,
			};

			product1.Author = author1;
			product2.Author = author1;
			product3.Author = author1;
			product4.Author = author7;
			product5.Author = author2;
			product6.Author = author2;
			product7.Author = author3;
			product8.Author = author4;
			product9.Author = author5;

			Domains.Add(author1);
			Domains.Add(author2);
			Domains.Add(author3);
			Domains.Add(author4);
			Domains.Add(author5);
			Domains.Add(author6);
			Domains.Add(author7);

			#endregion Domains

			AccountColors.Add(DefaultDataMock.CreateColor("#6601e3", Shell.Current.BackgroundColor));
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());
			AccountColors.Add(DefaultDataMock.CreateRandomColor());

			Emissions.Add(new Emission { ETH = "100", Number = 1, UNT = "100" });
			Emissions.Add(new Emission { ETH = "1000", Number = 2, UNT = "1000" });
			Emissions.Add(new Emission { ETH = "10000", Number = 3, UNT = "10000" });
			Emissions.Add(new Emission { ETH = "100000", Number = 4, UNT = "10000" });

			Notifications.Add(DefaultDataMock.CreateNotification(NotificationSeverity.High, NotificationType.ProductOperations));
			Notifications.Add(DefaultDataMock.CreateNotification(NotificationSeverity.Low, NotificationType.SystemEvent));
			Notifications.Add(DefaultDataMock.CreateNotification(NotificationSeverity.Mid, NotificationType.AuthorOperations));
			Notifications.Add(DefaultDataMock.CreateNotification(NotificationSeverity.High, NotificationType.TokenOperations));
			Notifications.Add(DefaultDataMock.CreateNotification(NotificationSeverity.Low, NotificationType.Server));
			Notifications.Add(DefaultDataMock.CreateNotification(NotificationSeverity.Mid, NotificationType.Wallet));
			Notifications.Add(DefaultDataMock.CreateNotification(NotificationSeverity.High, NotificationType.Server));

			HelpQuestions.Add(Strings.Resources.HelpLine1);
			HelpQuestions.Add(Strings.Resources.HelpLine2);
			HelpQuestions.Add(Strings.Resources.HelpLine3);
			HelpQuestions.Add(Strings.Resources.HelpLine4);
			HelpQuestions.Add(Strings.Resources.HelpLine5);
			HelpQuestions.Add(Strings.Resources.HelpLine6);
			HelpQuestions.Add(Strings.Resources.HelpLine7);
			HelpQuestions.Add(Strings.Resources.HelpLine8);
			HelpQuestions.Add(Strings.Resources.HelpLine9);
			HelpQuestions.Add(Strings.Resources.HelpLine10);
			HelpQuestions.Add(Strings.Resources.HelpLine11);
			HelpQuestions.Add(Strings.Resources.HelpLine12);
			HelpQuestions.Add(Strings.Resources.HelpLine13);
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "InitializeMockData Exception: {Ex}", ex.Message);
		}
	}
}
