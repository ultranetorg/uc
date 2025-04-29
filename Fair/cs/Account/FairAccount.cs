namespace Uccs.Fair;

public class FairAccount : Account
{
	public string					Nickname { get; set; }
	public AutoId[]				Authors  { get; set; }
	public AutoId[]				Sites  { get; set; }
	public AutoId[]				Reviews  { get; set; }
	public AutoId[]				FavoriteSites  { get; set; }
	public int						Approvals  { get; set; }
	public int						Rejections  { get; set; }
	public AutoId					AllocationSponsor { get; set; }
	public byte						AllocationSponsorClass  { get; set; }

	public FairAccount()
	{
	}

	public FairAccount(Mcv mcv) : base(mcv)
	{
	}

	public override Account Clone()
	{
		var a = base.Clone() as FairAccount;

		a.Nickname				 = Nickname;
		a.Authors				 = Authors;
		a.Sites					 = Sites;
		a.Reviews				 = Reviews;
		a.Approvals				 = Approvals;
		a.Rejections			 = Rejections;
		a.AllocationSponsor		 = AllocationSponsor;
		a.AllocationSponsorClass = AllocationSponsorClass;
		a.FavoriteSites			 = FavoriteSites;

		return a;
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.WriteUtf8(Nickname);
		writer.Write(Authors);
		writer.Write(Sites);
		writer.Write(Reviews);
		writer.Write(FavoriteSites);

		writer.WriteNullable(AllocationSponsor);
		
		if(AllocationSponsor != null)
		{
			writer.Write(AllocationSponsorClass);
			writer.Write7BitEncodedInt(Approvals);
			writer.Write7BitEncodedInt(Rejections);
		}
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		Nickname				= reader.ReadUtf8();
		Authors					= reader.ReadArray<AutoId>();
		Sites					= reader.ReadArray<AutoId>();
		Reviews					= reader.ReadArray<AutoId>();
		FavoriteSites			= reader.ReadArray<AutoId>();

		AllocationSponsor		= reader.ReadNullable<AutoId>();
		
		if(AllocationSponsor != null)
		{
			AllocationSponsorClass	= reader.ReadByte();
			Approvals				= reader.Read7BitEncodedInt();
			Rejections				= reader.Read7BitEncodedInt();
		}
	}
}
