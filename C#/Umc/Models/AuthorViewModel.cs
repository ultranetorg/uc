using UC.Net;

namespace UC.Umc.Models;

public class AuthorViewModel
{
	protected AuthorEntry _entry;

    public int						Id { get; internal set; }
    public string					Name { get; internal set; } // => _entry.Name;
    public string					Title { get; internal set; } // => _entry.Title;
    public decimal					CurrentBid { get; internal set; } // => _entry.LastBid;
    public AuthorStatus				Status { get; internal set; }
    public BidStatus				BidStatus { get; internal set; }
    public string					ActiveDue { get; internal set; }
    public int						DaysLeft { get; internal set; }

    public AccountViewModel			Account { get; internal set; }
    public IList<ProductViewModel>	Products = new List<ProductViewModel>();

	public string					DisplayLine1 {
		get
		{
			switch(Status)
			{
				case AuthorStatus.Auction:
					return BidStatus.ToString();
				case AuthorStatus.Watched:
					return $"Current bid: {CurrentBid}";
				case AuthorStatus.Owned:
					return $"Expires in: {ActiveDue}";
				case AuthorStatus.Reserved:
					return $"Expires in: {ActiveDue}";
				default:
					return string.Empty;
			}
		}
	}

	public string					DisplayLine2 {
		get
		{
			switch(Status)
			{
				case AuthorStatus.Auction:
					return $"Days left: {DaysLeft}";
				case AuthorStatus.Watched:
					return $"Days left: {DaysLeft}";
				case AuthorStatus.Owned:
					return Title;
				case AuthorStatus.Reserved:
					return Title;
				default:
					return string.Empty;
			}
		}
	}

	public AuthorViewModel()
	{
	}

	public AuthorViewModel(AuthorEntry entry)
	{
		_entry = entry;
	}
}

public enum AuthorStatus
{
	Auction, Watched, Owned, Free, Reserved, Hidden
}

public enum BidStatus
{
    None, Higher, Lower
}
