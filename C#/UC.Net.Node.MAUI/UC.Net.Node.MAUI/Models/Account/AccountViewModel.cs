namespace UC.Net.Node.MAUI.Models;

public class AccountViewModel
{
    public string Name { get; set; }

    public GradientBrush Color { get; set; }

    public bool ShowOnDashboard { get; set; } = true;

	public AccountEntry Entry { get; private set; }

	public AccountViewModel(AccountEntry entry)
	{
		Entry = entry;
	}

	public AccountViewModel(string address)
	{
		Address = address;
	}

	public string Address { get; set; }
    // public string Address => Entry.Account.ToString();

	public decimal Balance { get; set; }
	//  public decimal Balance {
	//	get => (decimal) Entry.Balance;
	//	set => Entry.Balance = new Coin(value); 
	//}
	
    public IList<string> Authors { get; set; } = new List<string>();
    //public List<string> Authors => Entry.Authors;
	
    public HashSet<int> Transactions { get; set; } = new HashSet<int>();
    // public HashSet<int> Transactions => Entry.Transactions;

	// lets say 1 unts = $1 unless we can recieve rate
	public string DisplayAmount => $"{Math.Round(Balance)} UNT (${Math.Round(Balance)})";

	public string IconCode => Address?[2..6];
}