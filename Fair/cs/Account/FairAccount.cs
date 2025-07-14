namespace Uccs.Fair;

public class FairAccount : Account
{
	public string					Nickname { get; set; }
	public AutoId[]					Authors  { get; set; }
	public AutoId[]					Sites  { get; set; }
	public AutoId[]					Reviews  { get; set; }
	public AutoId[]					FavoriteSites  { get; set; }
	public int						Approvals  { get; set; }
	public int						Rejections  { get; set; }
	public EntityAddress			AllocationSponsor { get; set; }
	public byte[]					Avatar  { get; set; }

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
		a.FavoriteSites			 = FavoriteSites;
		a.Avatar				 = Avatar;

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
		writer.WriteBytes(Avatar);

		writer.WriteNullable(AllocationSponsor);
		
		if(AllocationSponsor != null)
		{
			writer.Write7BitEncodedInt(Approvals);
			writer.Write7BitEncodedInt(Rejections);
		}

		//(this as ISpaceConsumer).WriteSpaceConsumer(writer);
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		Nickname				= reader.ReadUtf8();
		Authors					= reader.ReadArray<AutoId>();
		Sites					= reader.ReadArray<AutoId>();
		Reviews					= reader.ReadArray<AutoId>();
		FavoriteSites			= reader.ReadArray<AutoId>();
		Avatar					= reader.ReadBytes();

		AllocationSponsor		= reader.ReadNullable<EntityAddress>();
		
		if(AllocationSponsor != null)
		{
			Approvals			= reader.Read7BitEncodedInt();
			Rejections			= reader.Read7BitEncodedInt();
		}

		//(this as ISpaceConsumer).ReadSpaceConsumer(reader);
	}
}
