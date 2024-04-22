using System.IO;
using System.Text.RegularExpressions;

namespace Uccs.Net
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

	public class Domain : IBinarySerializable
	{
		//public const int			ExclusiveLengthMax = 12;
		public const int			NameLengthMin = 1;
		public const int			NameLengthMax = 256;
		public const char			NormalPrefix = '_';
		public const char			CountryPrefix = '~';

		public static readonly Time	AuctionMinimalDuration = Time.FromDays(365);
		public static readonly Time	Prolongation = Time.FromDays(30);
		public static readonly Time	WinnerRegistrationPeriod = Time.FromDays(30);
		public static readonly Time	RenewaPeriod = Time.FromDays(365);
		public Time					AuctionEnd => Time.Max(FirstBidTime + AuctionMinimalDuration, LastBidTime + Prolongation);

		public EntityId				Id { get; set; }
		public string				Name { get; set; }
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

		public static bool Valid(string name)
		{
			if(name == null)
				return false;

			if(name.Length < NameLengthMin || name.Length > NameLengthMax)
				return false;

			if(Regex.Match(name, $@"^[a-z0-9{NormalPrefix}]+$").Success == false)
				return false;

			return true;
		}

		public static bool IsExclusive(string name) => name[0] != NormalPrefix; 

		public static bool IsExpired(Domain a, Time time) 
		{
			return	a.LastWinner != null && a.Owner == null && time > a.AuctionEnd + WinnerRegistrationPeriod ||  /// winner has not registered since the end of auction, restart the auction
					a.Owner != null && time > a.Expiration;	 /// owner has not renewed, restart the auction
		}

		public static bool CanRegister(string name, Domain domain, Time time, AccountAddress by)
		{
			return	domain == null && !IsExclusive(name) || /// available
					domain != null && !IsExclusive(name) && domain.Owner != null && time > domain.Expiration || /// not renewed by current owner
					domain != null && IsExclusive(name) && domain.Owner == null && domain.LastWinner == by &&	
						time > domain.FirstBidTime + AuctionMinimalDuration && /// auction lasts minimum specified period
						time > domain.LastBidTime + Prolongation && /// wait until prolongation is over
						time < domain.AuctionEnd + WinnerRegistrationPeriod || /// auction is over and a winner can register an domain during special period
					domain != null && domain.Owner == by &&	time > domain.Expiration - RenewaPeriod && /// renewal by owner: renewal is allowed during last year olny
															time <= domain.Expiration
				  ;
		}

		public static bool CanBid(string name, Domain domain, Time time)
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

		public void Write(BinaryWriter w)
		{
			var f = DomainFlag.None;
			
			if(LastWinner != null)	f |= DomainFlag.Auction;
			if(Owner != null)		f |= DomainFlag.Owned;
			if(ComOwner != null)	f |= DomainFlag.ComOwned;
			if(OrgOwner != null)	f |= DomainFlag.OrgOwned;
			if(NetOwner != null)	f |= DomainFlag.NetOwned;

			w.Write((byte)f);
			w.WriteUtf8(Name);
			w.Write7BitEncodedInt(NextResourceId);
			w.Write7BitEncodedInt(SpaceReserved);
			w.Write7BitEncodedInt(SpaceUsed);

			if(IsExclusive(Name))
			{
				if(f.HasFlag(DomainFlag.Auction))
				{
					w.Write(FirstBidTime);
					w.Write(LastWinner);
					w.Write(LastBidTime);
					w.Write(LastBid);
				}

				if(f.HasFlag(DomainFlag.ComOwned))	w.Write(ComOwner);
				if(f.HasFlag(DomainFlag.OrgOwned))	w.Write(OrgOwner);
				if(f.HasFlag(DomainFlag.NetOwned))	w.Write(NetOwner);
			}

			if(f.HasFlag(DomainFlag.Owned))
			{
				w.Write(Owner);
				w.Write(Expiration);
			}

		}

		public void Read(BinaryReader reader)
		{
			var f			= (DomainFlag)reader.ReadByte();
			Name			= reader.ReadUtf8();
			NextResourceId	= reader.Read7BitEncodedInt();
			SpaceReserved	= (short)reader.Read7BitEncodedInt();
			SpaceUsed		= (short)reader.Read7BitEncodedInt();

			if(IsExclusive(Name))
			{
				if(f.HasFlag(DomainFlag.Auction))
				{
					FirstBidTime	= reader.Read<Time>();
					LastWinner		= reader.ReadAccount();
					LastBidTime		= reader.Read<Time>();
					LastBid			= reader.Read<Money>();
				}

				if(f.HasFlag(DomainFlag.ComOwned))	ComOwner = reader.Read<AccountAddress>();
				if(f.HasFlag(DomainFlag.OrgOwned))	OrgOwner = reader.Read<AccountAddress>();
				if(f.HasFlag(DomainFlag.NetOwned))	NetOwner = reader.Read<AccountAddress>();
			}

			if(f.HasFlag(DomainFlag.Owned))
			{
				Owner		= reader.ReadAccount();
				Expiration	= reader.Read<Time>();
			}
		}
	}
}
