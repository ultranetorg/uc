namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class TransactionsViewModel : BaseTransactionsViewModel
{	
	[ObservableProperty]
    private Transaction _selectedItem;
    
	[ObservableProperty]
    private CustomCollection<Transaction> _transactions = new();

    public TransactionsViewModel(ILogger<TransactionsViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	#region Fake Data

	private void FillFakeData()
	{
		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 540,
			Name = "Register ultranetorg author",
			Status = TransactionStatus.Pending,
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
		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Sent to 0xAA...FF00",
			Status = TransactionStatus.Sent,
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
		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Recieve from 0xAA...FF00",
			Status = TransactionStatus.Received,
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
		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 540,
			Name = "Sent to 0xAA...FF00",
			Status = TransactionStatus.Sent,
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
		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Register ultranetorg author",
			Status = TransactionStatus.Pending,
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
		Transactions.Add(new Transaction
		{
			FromId = Generator.GenerateUniqueID(6),
			ToId = Generator.GenerateUniqueID(6),
			Unt = 590,
			Name = "Recieve from 0xAA...FF00",
			Status = TransactionStatus.Received,
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
	}

	#endregion Fake Data
}
