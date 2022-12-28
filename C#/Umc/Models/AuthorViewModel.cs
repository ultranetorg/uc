using UC.Net;

namespace UC.Umc.Models;

public class AuthorViewModel
{
	protected AuthorEntry _entry;

    public int						Id { get; internal set; }
    public string					Name { get; internal set; } // => _entry.Name;
    public string					Title { get; internal set; } // => _entry.Title;
    public decimal					CurrentBid { get; internal set; } // => _entry.LastBid;
    public BidStatus				BidStatus { get; internal set; }
    public string					ActiveDue { get; internal set; }

    public AccountViewModel			Account { get; internal set; }
    public IList<ProductViewModel>	Products = new List<ProductViewModel>();

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
