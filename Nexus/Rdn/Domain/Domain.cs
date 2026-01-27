using System.Text.RegularExpressions;

namespace Uccs.Rdn;

public enum DomainFlag : byte
{
	None, 
	Owned		= 0b_______1, 
	Free		= 0b______10, 
	ChildNet	= 0b__100000, 
}

public enum DomainChildPolicy : byte
{
	None, 
	FullOwnership	= 1, 
	FullFreedom		= 2, 
	Programmatic	= 0b11111111, 
}

public enum NnStatus
{
	None,
	Initialized,
	BlockRecieved,
	BlockSent
}

public class Domain : IBinarySerializable, ISpaceConsumer, ITableEntry, IExpirable
{
	//public const int				ExclusiveLengthMax = 12;
	public const int				NameLengthMin = 1;
	public const int				NameLengthMax = 256;
	//public const char				NormalPrefix = '_';
	public const char				National = '~';
	public const char				Subdomain = '~';

	public static readonly string[] ExclusiveTlds = ["com", "org", "net", "info", "biz"];

	//public static readonly Time	AuctionMinimalDuration = Time.FromDays(365);
	//public static readonly Time	Prolongation = Time.FromDays(30);
	//public static readonly Time	WinnerRegistrationPeriod = Time.FromDays(30);
	//public static readonly short	RenewalPeriod = (short)(Time.FromDays(365).Days);
	//public Time					AuctionEnd => Time.Max(FirstBidTime + AuctionMinimalDuration, LastBidTime + Prolongation);

	public AutoId					Id { get; set; }
	public string					Address { get; set; }
	public AutoId					Owner { get; set; }
	public bool						Free { get; set; }
	//public Time					FirstBidTime { get; set; } = Time.Empty;
	//public AutoId					LastWinner { get; set; }
	//public long					LastBid { get; set; }
	//public Time					LastBidTime { get; set; } = Time.Empty;
	
	public short					Expiration { get; set; }
	public long						Space { get; set; }
	
	public DomainChildPolicy		ParentPolicy { get; set; }
	public NnpState					NnChildNet { get; set; }
	public byte[]					NnSelfHash { get; set; }

	public EntityId					Key => Id;
	public bool						Deleted { get; set; }
	Mcv								Mcv;

	public static bool				IsRoot(string name) => !name.Contains('.'); 
	public static bool				IsChild(string name) => name.Contains('.'); 
	public bool						IsFree(Execution execution) => Free;
	public static string			GetParent(string name) => name.Substring(name.IndexOf('.') + 1); 
	public static string			GetName(string name) => name.Substring(0, name.IndexOf('.'));

	public Domain()
	{
	}

	public Domain(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Address}, {Id}, Owner={Owner}, {Expiration}";
	}

	public object Clone()
	{
		return new Domain(Mcv){	Id = Id,
								Address = Address,
								Owner = Owner,
								Expiration = Expiration,
								Free = Free,
								Space = Space,
								NnChildNet = NnChildNet,
								NnSelfHash = NnSelfHash};
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public static bool Valid(string name)
	{
		if(name == null)
			return false;

		if(name.Length < NameLengthMin || name.Length > NameLengthMax)
			return false;

		if(Regex.Match(name, $@"^[a-z0-9\.]+[a-z0-9{Subdomain}{National}]*$").Success == false)
			return false;

		return true;
	}
			
	public static string GetRoot(string name)
	{
		var i = name.LastIndexOf('.');

		return i == -1 ? name : name.Substring(i + 1);
	}

	public static bool IsOwner(Domain domain, User account, Time time)
	{
		return domain.Owner == account.Id && !domain.IsExpired(time);
	}

	public bool IsExpired(Time time)
	{
		return	//LastWinner != null && Owner == null && time > AuctionEnd + WinnerRegistrationPeriod ||  /// winner has not registered since the end of auction, restart the auction
									  Owner != null && time.Days >= Expiration;	 /// owner has not renewed, restart the auction
	}

	public bool CanRenew(User owner, Time time, Time duration)
	{
		return  Owner == owner.Id && ((IExpirable)this).CanRenew(time, duration);
	}

	public static bool CanRegister(string name, Domain domain, Time time, User by)
	{
		return	domain == null || /// available
				domain != null && domain.Owner != null && time.Days >= domain.Expiration; /// not renewed by current owner
				
	}

	public void ResetFreeIfNeeded(Execution execution)
	{
		if(Free && Space > execution.Net.FreeSpaceMaximum)
			Free = false;
	}

//	public static bool CanBid(Domain domain, Time time)
//	{
// 		if(!domain.IsExpired(time))
// 		{
//			if(domain.LastWinner == null) /// first bid
//			{
//				return true;
//			}
//			//else if(time - domain.FirstBidTime < AuctionMinimalDuration || time - domain.LastBidTime < Prolongation)
//			else if(time < domain.AuctionEnd)
//			{
//				return true;
//			}
// 		} 
// 		else
// 		{
//			return true;
//		}
//
//		return false;
//	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);

		var f = DomainFlag.None;
		
		//if(LastWinner != null)	f |= DomainFlag.Auction;
		if(Owner != null)		f |= DomainFlag.Owned;
		if(NnChildNet != null)	f |= DomainFlag.ChildNet;
		if(Free)				f |= DomainFlag.Free;

		writer.Write((byte)f);
		writer.WriteUtf8(Address);
		writer.Write7BitEncodedInt64(Space);

		if(f.HasFlag(DomainFlag.Owned))
		{
			writer.Write(Owner);
			writer.Write(Expiration);
		}

		if(IsChild(Address))
		{
			writer.Write((byte)ParentPolicy);
		}

		if(f.HasFlag(DomainFlag.ChildNet))
		{
			writer.Write(NnChildNet);
			writer.Write(NnSelfHash);
		}
	}

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<AutoId>();
		var f		= (DomainFlag)reader.ReadByte();
		Address		= reader.ReadUtf8();
		Space		= reader.Read7BitEncodedInt64();
		Free		= f.HasFlag(DomainFlag.Free);

		if(f.HasFlag(DomainFlag.Owned))
		{
			Owner		= reader.Read<AutoId>();
			Expiration	= reader.ReadInt16();
		}

		if(IsChild(Address))
		{
			ParentPolicy = (DomainChildPolicy)reader.ReadByte();
		}

		if(f.HasFlag(DomainFlag.ChildNet))
		{
			NnChildNet	= reader.Read<NnpState>();
			NnSelfHash = reader.ReadHash();
		}
	}
}
