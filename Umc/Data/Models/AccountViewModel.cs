using UC.Net;

namespace UC.Umc.Models;

public partial class AccountViewModel : ObservableObject
{
	[ObservableProperty]
	private bool			_isSelected;
	[ObservableProperty]
	private GradientBrush	_color;
	[ObservableProperty]
	protected Account		_account;

	public string			Name { get; set; }
	public decimal			Balance { get; set; }
	public string			Address { get; set; }
	public bool				HideOnDashboard { get; set; }
	
    public IList<string>	Authors { get; set; } = new List<string>();
    public HashSet<int>		Transactions { get; set; } = new HashSet<int>();

	public decimal			RoundedBalance => Math.Round(Balance);
	// lets say 1 unts = $1 unless we can recieve rate
	public string			DisplayAmount => $"{RoundedBalance} UNT (${RoundedBalance})";
	public string			DisplayAmountShort => $"{RoundedBalance} UNT";
	public bool				ShowAmount { get; set; } = true;
	public string			IconCode => string.IsNullOrEmpty(Address) ? string.Empty : Address?[2..6];

	public AccountViewModel(Account account)
	{
		_account = account;
	}

	// WBD
	public AccountViewModel(string address)
	{
		Address = address;
	}
}