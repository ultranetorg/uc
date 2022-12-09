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

	public decimal			RoundedBalance => Math.Round(Balance);
	// lets say 1 unts = $1 unless we can recieve rate
	public string			DisplayAmount => $"{RoundedBalance} UNT (${RoundedBalance})";
	public string			IconCode => Address?[2..6];
	public bool				IsSelected { get; set; }

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