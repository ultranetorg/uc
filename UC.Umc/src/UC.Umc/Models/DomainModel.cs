using UC.Umc.Common.Helpers;

namespace UC.Umc.Models;

public partial class DomainModel : ObservableObject
{
	[ObservableProperty]
	private bool					_isSelected;
	protected UC.DomainModels.DomainModel	_entry;

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

	public AccountModel			Account { get; internal set; }
	public IList<ResourceModel>	Products = new List<ResourceModel>();
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
					return BidStatus == BidStatus.Higher ? "You have higher bid" : "You are not leading";
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

	public string DisplayLine2
	{
		get
		{
			switch (Status)
			{
				case AuthorStatus.Auction:
					return $"Days left: {CommonHelper.GetDaysLeft(AuctionEndDate)}";
				case AuthorStatus.Watched:
					return $"Days left: {CommonHelper.GetDaysLeft(AuctionEndDate)}";
				case AuthorStatus.Owned:
					return Title;
				case AuthorStatus.Reserved:
					return Title;
				default:
					return string.Empty;
			}
		}
	}

	#endregion Display

	public DomainModel()
	{
	}

	public DomainModel(UC.DomainModels.DomainModel entry)
	{
		_entry = entry;
	}
}

public class Bid
{
	public decimal			Amount { get; internal set; }
	public DateTime			Date { get; internal set; }
	public string			BidBy { get; internal set; }
}

public enum AuthorStatus
{
	Auction, Watched, Owned, Free, Reserved, Hidden
}

public enum BidStatus
{
	None, Higher, Lower
}
