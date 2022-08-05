namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class TransactionsBViewModel : BaseTransactionsViewModel
{
    private TransactionList TransactionsThisWeek;
    private TransactionList TransactionsLastWeek;

	[ObservableProperty]
    private CustomCollection<TransactionList> _transactions = new();

	[ObservableProperty]
    private Wallet _wallet;

    public TransactionsBViewModel(ILogger<TransactionsBViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	#region Fake Data
	
	private void FillFakeData()
	{
		TransactionsLastWeek = new TransactionList("Last week");
		TransactionsThisWeek = new TransactionList("This week");
		Transactions.Add(TransactionsThisWeek);
		Transactions.Add(TransactionsLastWeek);

		_wallet = new()
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "47F0",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#6601e3"),
		};

		TransactionsThisWeek.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 540,
			Name = "Register ultranetorg author",
			Status = TransactionsStatus.Pending,
			USD = 185.35,
			Hash = "0x63FaC9201494f0bd17B9892B9fad52fe3BD377",
			Wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				Unts = 5005,
				IconCode = "47F0",
				Name = "Main ultranet wallet",
				AccountColor = Color.FromArgb("#6601e3"),
			}
		});
		TransactionsThisWeek.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Sent to 0xAA...FF00",
			Status = TransactionsStatus.Sent,
			USD = 85.33,
			Hash = "0x63FaC9201494f0bd17B9892B9fad52fe3BD377",
			Wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				Unts = 5005,
				IconCode = "47F0",
				Name = "Main ultranet wallet",
				AccountColor = Color.FromArgb("#6601e3"),
			}
		});
		TransactionsThisWeek.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Recieve from 0xAA...FF00",
			Status = TransactionsStatus.Received,
			USD = 85.33,
			Hash = "0x63FaC9201494f0bd17B9892B9fad52fe3BD377",
			Wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				Unts = 5005,
				IconCode = "47F0",
				Name = "Main ultranet wallet",
				AccountColor = Color.FromArgb("#6601e3"),
			}
		});
		TransactionsThisWeek.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 540,
			Name = "Sent to 0xAA...FF00",
			Status = TransactionsStatus.Sent,
			USD = 185.44,
			Hash = "0x63FaC9201494f0bd17B9892B9fad52fe3BD377",
			Wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				Unts = 5005,
				IconCode = "47F0",
				Name = "Main ultranet wallet",
				AccountColor = Color.FromArgb("#6601e3"),
			}
		});
		TransactionsLastWeek.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Register ultranetorg author",
			Status = TransactionsStatus.Pending,
			USD = 85.33,
			Hash = "0x63FaC9201494f0bd17B9892B9fad52fe3BD377",
			Wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				Unts = 5005,
				IconCode = "47F0",
				Name = "Main ultranet wallet",
				AccountColor = Color.FromArgb("#6601e3"),
			}
		});
		TransactionsLastWeek.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Recieve from 0xAA...FF00",
			Status = TransactionsStatus.Received,
			USD = 185.55,
			Wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				Unts = 5005,
				IconCode = "47F0",
				Name = "Main ultranet wallet",
				AccountColor = Color.FromArgb("#6601e3"),
			}
		});
		TransactionsLastWeek.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Register ultranetorg author",
			Status = TransactionsStatus.Pending,
			USD = 85.33,
			Hash = "0x63FaC9201494f0bd17B9892B9fad52fe3BD377",
			Wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				Unts = 5005,
				IconCode = "47F0",
				Name = "Main ultranet wallet",
				AccountColor = Color.FromArgb("#6601e3"),
			}
		});
	}

	#endregion Fake Data
}
