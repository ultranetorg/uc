using System.IO;
using System.Text.RegularExpressions;

namespace Uccs.Net
{
	public enum AuthorFlag : byte
	{
		None, 
		Owned		= 0b____1, 
		Auction		= 0b___10, 
		ComOwned	= 0b__100, 
		OrgOwned	= 0b_1000, 
		NetOwned	= 0b10000, 
	}

	public class Author : IBinarySerializable
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
		public AccountAddress		ComOwner  { get; set; }
		public AccountAddress		OrgOwner  { get; set; }
		public AccountAddress		NetOwner  { get; set; }
		public Time					FirstBidTime { get; set; } = Time.Empty;
		public AccountAddress		LastWinner { get; set; }
		public Money				LastBid { get; set; }
		public Time					LastBidTime { get; set; }
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

		public static bool IsExpired(Author a, Time time) 
		{
			return	a.LastWinner != null && a.Owner == null && time > a.AuctionEnd + WinnerRegistrationPeriod ||  /// winner has not registered since the end of auction, restart the auction
					a.Owner != null && time > a.Expiration;	 /// owner has not renewed, restart the auction
		}

		public static bool CanRegister(string name, Author author, Time time, AccountAddress by)
		{
			return	author == null && !IsExclusive(name) || /// available
					author != null && !IsExclusive(name) && author.Owner != null && time > author.Expiration || /// not renewed by current owner
					author != null && IsExclusive(name) && author.Owner == null && author.LastWinner == by &&	
						time > author.FirstBidTime + AuctionMinimalDuration && /// auction lasts minimum specified period
						time > author.LastBidTime + Prolongation && /// wait until prolongation is over
						time < author.AuctionEnd + WinnerRegistrationPeriod || /// auction is over and a winner can register an author during special period
					author != null && author.Owner == by &&	time > author.Expiration - RenewaPeriod && /// renewal by owner: renewal is allowed during last year olny
															time <= author.Expiration
				  ;
		}

		public static bool CanBid(string name, Author author, Time time)
		{
 			if(!IsExpired(author, time))
 			{
				if(author.LastWinner == null) /// first bid
				{
					return true;
				}
				//else if(time - author.FirstBidTime < AuctionMinimalDuration || time - author.LastBidTime < Prolongation)
				else if(time < author.AuctionEnd)
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
			var f = AuthorFlag.None;
			
			if(LastWinner != null)	f |= AuthorFlag.Auction;
			if(Owner != null)		f |= AuthorFlag.Owned;
			if(ComOwner != null)	f |= AuthorFlag.ComOwned;
			if(OrgOwner != null)	f |= AuthorFlag.OrgOwned;
			if(NetOwner != null)	f |= AuthorFlag.NetOwned;

			w.Write((byte)f);
			w.WriteUtf8(Name);
			w.Write7BitEncodedInt(NextResourceId);
			w.Write7BitEncodedInt(SpaceReserved);
			w.Write7BitEncodedInt(SpaceUsed);

			if(IsExclusive(Name))
			{
				if(f.HasFlag(AuthorFlag.Auction))
				{
					w.Write(FirstBidTime);
					w.Write(LastWinner);
					w.Write(LastBidTime);
					w.Write(LastBid);
				}

				if(f.HasFlag(AuthorFlag.ComOwned))	w.Write(ComOwner);
				if(f.HasFlag(AuthorFlag.OrgOwned))	w.Write(OrgOwner);
				if(f.HasFlag(AuthorFlag.NetOwned))	w.Write(NetOwner);
			}

			if(f.HasFlag(AuthorFlag.Owned))
			{
				w.Write(Owner);
				w.Write(Expiration);
			}
		}

		public void Read(BinaryReader reader)
		{
			var f			= (AuthorFlag)reader.ReadByte();
			Name			= reader.ReadUtf8();
			NextResourceId	= reader.Read7BitEncodedInt();
			SpaceReserved	= (short)reader.Read7BitEncodedInt();
			SpaceUsed		= (short)reader.Read7BitEncodedInt();

			if(IsExclusive(Name))
			{
				if(f.HasFlag(AuthorFlag.Auction))
				{
					FirstBidTime	= reader.Read<Time>();
					LastWinner		= reader.ReadAccount();
					LastBidTime		= reader.Read<Time>();
					LastBid			= reader.Read<Money>();
				}

				if(f.HasFlag(AuthorFlag.ComOwned))	ComOwner = reader.Read<AccountAddress>();
				if(f.HasFlag(AuthorFlag.OrgOwned))	OrgOwner = reader.Read<AccountAddress>();
				if(f.HasFlag(AuthorFlag.NetOwned))	NetOwner = reader.Read<AccountAddress>();
			}

			if(f.HasFlag(AuthorFlag.Owned))
			{
				Owner		= reader.ReadAccount();
				Expiration	= reader.Read<Time>();
			}
		}
	}
}
