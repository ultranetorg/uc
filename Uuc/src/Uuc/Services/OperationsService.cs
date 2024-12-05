using Uuc.Models.Accounts;

namespace Uuc.Services;

public class OperationsService : IOperationsService
{
	private readonly IList<Operation> _operations = new List<Operation>
	{
		new Operation
		{
			Id = "0-0-0",
			SignerAddress = "0",
			TransactionId = "0-0",
		},
		new Operation
		{
			Id = "0-0-1",
			SignerAddress = "1",
			TransactionId = "0-1",
		},
		new Operation
		{
			Id = "0-0-2",
			SignerAddress = "2",
			TransactionId = "0-2",
		},
		new Operation
		{
			Id = "0-1-0",
			SignerAddress = "3",
			TransactionId = "1-0",
		},
		new Operation
		{
			Id = "0-1-1",
			SignerAddress = "4",
			TransactionId = "1-1",
		},
		new Operation
		{
			Id = "0-1-2",
			SignerAddress = "5",
			TransactionId = "1-2",
		},
		new Operation
		{
			Id = "0-1-3",
			SignerAddress = "6",
			TransactionId = "1-3",
		},
		new Operation
		{
			Id = "0-1-4",
			SignerAddress = "7",
			TransactionId = "1-4",
		},
		new Operation
		{
			Id = "0-1-5",
			SignerAddress = "8",
			TransactionId = "1-5",
		},
		new Operation
		{
			Id = "0-2-0",
			SignerAddress = "9",
			TransactionId = "2-0",
		},
		new Operation
		{
			Id = "0-2-1",
			SignerAddress = "A",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			SignerAddress = "B",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			SignerAddress = "C",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			SignerAddress = "D",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			SignerAddress = "E",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			SignerAddress = "F",
			TransactionId = "2-1",
		},
	};
	public async Task<IList<Operation>?> ListAllAsync(CancellationToken cancellationToken = default)
	{
		return _operations;
	}

	public async Task<IList<Operation>?> ListByAccountAddressAsync(string accountAddress, CancellationToken cancellationToken = default)
	{
		Guard.IsNotEmpty(accountAddress);

		var last3Chars = accountAddress.Substring(accountAddress.Length - 3);
		string unique = new (last3Chars.Distinct().ToArray());

		return _operations.Where(x => unique.Contains(x.SignerAddress)).Select(x =>
		{
			x.SignerAddress = accountAddress;
			return x;
		}).ToList();
	}
}
