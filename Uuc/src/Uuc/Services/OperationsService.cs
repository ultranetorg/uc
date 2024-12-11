using Uuc.Models.Accounts;

namespace Uuc.Services;

public class OperationsService : IOperationsService
{
	private readonly IList<Operation> _operations = new List<Operation>
	{
		new Operation
		{
			Id = "0-0-0",
			NetworkId = "96AD41DC-CA71-45CC-B27B-EAC673EE0E81",
			SignerAddress = "0",
			TransactionId = "0-0",
		},
		new Operation
		{
			Id = "0-0-1",
			NetworkId = "93B106AD-5922-4CCB-AE2A-B175DEE83502",
			SignerAddress = "1",
			TransactionId = "0-1",
		},
		new Operation
		{
			Id = "0-0-2",
			NetworkId = "9830DC4A-83D7-43C8-B662-B32E95B2FC89",
			SignerAddress = "2",
			TransactionId = "0-2",
		},
		new Operation
		{
			Id = "0-1-0",
			NetworkId = "79B7503C-9BDD-469A-A1B9-F4438F19CD6A",
			SignerAddress = "3",
			TransactionId = "1-0",
		},
		new Operation
		{
			Id = "0-1-1",
			NetworkId = "1B48E9E0-99C1-495A-A4AB-8242427AC229",
			SignerAddress = "4",
			TransactionId = "1-1",
		},
		new Operation
		{
			Id = "0-1-2",
			NetworkId = "57027AA8-C293-4A05-8255-146B656D83B7",
			SignerAddress = "5",
			TransactionId = "1-2",
		},
		new Operation
		{
			Id = "0-1-3",
			NetworkId = "9830DC4A-83D7-43C8-B662-B32E95B2FC89",
			SignerAddress = "6",
			TransactionId = "1-3",
		},
		new Operation
		{
			Id = "0-1-4",
			NetworkId = "7BE61B58-45A5-469D-B706-D54E51F75E43",
			SignerAddress = "7",
			TransactionId = "1-4",
		},
		new Operation
		{
			Id = "0-1-5",
			NetworkId = "96AD41DC-CA71-45CC-B27B-EAC673EE0E81",
			SignerAddress = "8",
			TransactionId = "1-5",
		},
		new Operation
		{
			Id = "0-2-0",
			NetworkId = "DD51A243-0405-4C0A-871D-1AF7F7AD86C9",
			SignerAddress = "9",
			TransactionId = "2-0",
		},
		new Operation
		{
			Id = "0-2-1",
			NetworkId = "50DD38A7-B257-4CF5-8A5D-3E7BBBB359BA",
			SignerAddress = "A",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			NetworkId = "59A86723-9C9E-4388-832C-B08FD224F308",
			SignerAddress = "B",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			NetworkId = "A4FCF2B0-382B-495E-A2D9-E594920C89A5",
			SignerAddress = "C",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			NetworkId = "D4723B37-FB11-440F-9683-FD620B667F27",
			SignerAddress = "D",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			NetworkId = "9BDA2EC0-2103-4D25-94DB-47BFD076B826",
			SignerAddress = "E",
			TransactionId = "2-1",
		},
		new Operation
		{
			Id = "0-2-1",
			NetworkId = "8DFD9444-37B1-4ACD-9FE7-1616B0E28E34",
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
