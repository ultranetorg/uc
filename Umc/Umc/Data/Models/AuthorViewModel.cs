using UC.Net;

namespace UC.Umc.Models;

public partial class AuthorViewModel : ObservableObject
{
	[ObservableProperty]
	private bool					_isSelected;
	protected AuthorEntry			_entry;
    public AccountViewModel			Account { get; internal set; }

    public int						Id { get; internal set; }
    public string					Name { get; internal set; } // => _entry.Name;
    public string					Title { get; internal set; } // => _entry.Title;
    public string					Owner { get; internal set; } // => _entry.Owner;
    public AuthorStatus				Status { get; internal set; }
    public DateTime					ExpirationDate { get; internal set; }

    public decimal					CurrentBid { get; internal set; } // => _entry.LastBid;
    public string					MaximumBidBy { get; internal set; } // => _entry.MaximumBidBy;
    public DateTime					AuctionEndDate { get; internal set; } // => _entry.MaximumBidBy;
    public BidStatus				BidStatus { get; internal set; }
    public IList<ProductViewModel>	Products = new List<ProductViewModel>();
	public IList<Bid>				BidsHistory = new List<Bid>();

	#region Display

	public bool						IsHidden { get; internal set; }
	public bool						ExpiresSoon => DateTime.Today.AddDays(30) > ExpirationDate; 
	public string					ActiveDue => $"{ExpirationDate.ToShortDateString()} ({CommonHelper.GetDaysLeft(ExpirationDate)} days)";
	public string					AuctionDue => $"{AuctionEndDate.ToShortDateString()} ({CommonHelper.GetDaysLeft(AuctionEndDate)} days)";

	public string DisplayLine1
	{
		get
		{
			switch (Status)
			{
				case AuthorStatus.Auction:
					return BidStatus == BidStatus.Higher
						? Properties.Author_Strings.Bid_Higher
						: Properties.Author_Strings.Bid_Lower;
				case AuthorStatus.Watched:
					return $"{Properties.Author_Strings.Bid_Current}: {CurrentBid}";
				case AuthorStatus.Owned:
				case AuthorStatus.Reserved:
					return $"{Properties.Author_Strings.Auction_Expires}: {ActiveDue}";
				default:
					return string.Empty;
			}
		}
	}

	public string DisplayLine2
	{
		get
		{
			switch (Status)
			{
				case AuthorStatus.Auction:
				case AuthorStatus.Watched:
					return $"{Properties.Author_Strings.Auction_DaysLeft}: {CommonHelper.GetDaysLeft(AuctionEndDate)}";
				case AuthorStatus.Owned:
				case AuthorStatus.Reserved:
					return Title;
				default:
					return string.Empty;
			}
		}
	}

	#endregion Display

	public AuthorViewModel()
	{
	}

	public AuthorViewModel(AuthorEntry entry)
	{
		_entry = entry;
	}
}

public class Bid
{
    public decimal		Amount { get; internal set; }
    public DateTime		Date { get; internal set; }
    public string		BidBy { get; internal set; }
}
