using System.Text.RegularExpressions;

namespace Uccs.Rdn
{
	public enum DomainFlag : byte
	{
		None, 
		Owned		= 0b_____1, 
		Auction		= 0b____10, 
		ComOwned	= 0b___100, 
		OrgOwned	= 0b__1000, 
		NetOwned	= 0b_10000, 
	}

	public enum DomainChildPolicy : byte
	{
		None, 
		FullOwnership	= 1, 
		FullFreedom		= 2, 
		Programmatic	= 0b11111111, 
	}

	public class Domain// : IBinarySerializable
	{
		//public const int			ExclusiveLengthMax = 12;
		public const int			NameLengthMin = 1;
		public const int			NameLengthMax = 256;
		public const char			NormalPrefix = '_';
		public const char			National = '~';

		public static readonly Time	AuctionMinimalDuration = Time.FromDays(365);
		public static readonly Time	Prolongation = Time.FromDays(30);
		public static readonly Time	WinnerRegistrationPeriod = Time.FromDays(30);
		public static readonly Time	RenewaPeriod = Time.FromDays(365);
		public Time					AuctionEnd => Time.Max(FirstBidTime + AuctionMinimalDuration, LastBidTime + Prolongation);

		public EntityId				Id { get; set; }
		public string				Address { get; set; }
		public AccountAddress		Owner { get; set; }
		public Time					Expiration { get; set; }
		public AccountAddress		ComOwner { get; set; }
		public AccountAddress		OrgOwner { get; set; }
		public AccountAddress		NetOwner { get; set; }
		public Time					FirstBidTime { get; set; } = Time.Empty;
		public AccountAddress		LastWinner { get; set; }
		public Money				LastBid { get; set; }
		public Time					LastBidTime { get; set; } = Time.Empty;
		public int					NextResourceId { get; set; }
		public short				SpaceReserved { get; set; }
		public short				SpaceUsed { get; set; }
		public DomainChildPolicy	ParentPolicy { get; set; }

		public static bool			IsWeb(string name) => IsRoot(name) && name[0] != NormalPrefix; 
		public static bool			IsRoot(string name) => name.IndexOf('.') == -1; 
		public static bool			IsChild(string name) => name.IndexOf('.') != -1; 
		public static string		GetParent(string name) => name.Substring(name.IndexOf('.') + 1); 
		public static string		GetName(string name) => name.Substring(0, name.IndexOf('.'));

		public static bool Valid(string name)
		{
			if(name == null)
				return false;

			if(name.Length < NameLengthMin || name.Length > NameLengthMax)
				return false;

			if(Regex.Match(name, $@"^[a-z0-9{NormalPrefix}{National}.]+$").Success == false)
				return false;

			return true;
		}
				
		public static string GetRoot(string name)
		{
			var i = name.LastIndexOf('.');

			return i == -1 ? name : name.Substring(i + 1);
		}

		public static bool IsOwner(Domain domain, AccountAddress account, Time time)
		{
			return domain.Owner == account && !IsExpired(domain, time);
		}

		public static bool IsExpired(Domain a, Time time) 
		{
			return	a.LastWinner != null && a.Owner == null && time > a.AuctionEnd + WinnerRegistrationPeriod ||  /// winner has not registered since the end of auction, restart the auction
					a.Owner != null && time > a.Expiration;	 /// owner has not renewed, restart the auction
		}

		public static bool CanRenew(Domain domain, AccountAddress by, Time time)
		{
			return  domain != null && domain.Owner == by &&	time > domain.Expiration - RenewaPeriod && /// renewal by owner: renewal is allowed during last year olny
															time <= domain.Expiration;
		}

		public static bool CanRegister(string name, Domain domain, Time time, AccountAddress by)
		{
			return	domain == null && !IsWeb(name) || /// available
					domain != null && !IsWeb(name) && domain.Owner != null && time > domain.Expiration || /// not renewed by current owner
					domain != null && IsWeb(name) && domain.Owner == null && domain.LastWinner == by &&	
						time > domain.FirstBidTime + AuctionMinimalDuration && /// auction lasts minimum specified period
						time > domain.LastBidTime + Prolongation && /// wait until prolongation is over
						time < domain.AuctionEnd + WinnerRegistrationPeriod || /// auction is over and a winner can register an domain during special period
					CanRenew(domain, by, time);
		}

		public static bool CanBid(Domain domain, Time time)
		{
 			if(!IsExpired(domain, time))
 			{
				if(domain.LastWinner == null) /// first bid
				{
					return true;
				}
				//else if(time - domain.FirstBidTime < AuctionMinimalDuration || time - domain.LastBidTime < Prolongation)
				else if(time < domain.AuctionEnd)
				{
					return true;
				}
 			} 
 			else
 			{
				return true;
			}

			return false;
		}
	}
}