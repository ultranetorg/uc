using UC.Net;

namespace UC.Umc.Models;

public class AccountViewModel
{
    public string			Name { get; set; }
    public GradientBrush	Color { get; set; }
    public bool				HideOnDashboard { get; set; }
	public decimal			Balance { get; set; }
	public Account			Account { get; private set; }
	public string			Address { get; set; }
    //public string			Address => Entry.Account.ToString();
	
    public IList<string>	Authors { get; set; } = new List<string>();
    public HashSet<int>		Transactions { get; set; } = new HashSet<int>();

	// lets say 1 unts = $1 unless we can recieve rate
	public string			DisplayAmount => $"{Math.Round(Balance)} UNT (${Math.Round(Balance)})";
	public string			IconCode => Address?[2..6];

	public AccountViewModel(Account account)
	{
		Account = account;
	}

	// WBD
	public AccountViewModel(string address)
	{
		Address = address;
	}
}