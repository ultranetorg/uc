namespace Uccs.Fair;

public class FairAccount : Account
{
	public string					Nickname { get; set; }
	public AutoId[]					Authors  { get; set; }
	public AutoId[]					ModeratedSites  { get; set; }
	public AutoId[]					Registrations  { get; set; }
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

		a.Nickname				= Nickname;
		a.Authors				= Authors;
		a.ModeratedSites		= ModeratedSites;
		a.Registrations			= Registrations;
		a.Reviews				= Reviews;
		a.Avatar				= Avatar;
		a.FavoriteSites			= FavoriteSites;
		a.Approvals				= Approvals;
		a.Rejections			= Rejections;
		a.AllocationSponsor		= AllocationSponsor;

		return a;
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.WriteUtf8(Nickname);
		writer.Write(Authors);
		writer.Write(ModeratedSites);
		writer.Write(Registrations);
		writer.Write(FavoriteSites);
		writer.Write(Reviews);
		writer.WriteBytes(Avatar);
		writer.Write7BitEncodedInt(Approvals);
		writer.Write7BitEncodedInt(Rejections);

		writer.WriteNullable(AllocationSponsor);
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		Nickname				= reader.ReadUtf8();
		Authors					= reader.ReadArray<AutoId>();
		ModeratedSites			= reader.ReadArray<AutoId>();
		Registrations			= reader.ReadArray<AutoId>();
		FavoriteSites			= reader.ReadArray<AutoId>();
		Reviews					= reader.ReadArray<AutoId>();
		Avatar					= reader.ReadBytes();
		Approvals				= reader.Read7BitEncodedInt();
		Rejections				= reader.Read7BitEncodedInt();

		AllocationSponsor		= reader.ReadNullable<EntityAddress>();
	}
}
