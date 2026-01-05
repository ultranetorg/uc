namespace Uccs.Fair;

public class FairUser : User
{
	public AutoId[]					Authors  { get; set; }
	public AutoId[]					ModeratedSites  { get; set; }
	public AutoId[]					Sites  { get; set; }
	public AutoId[]					Reviews  { get; set; }
	public AutoId[]					FavoriteSites  { get; set; }
	public int						Approvals  { get; set; }
	public int						Rejections  { get; set; }
	public byte[]					Avatar  { get; set; }

	public FairUser()
	{
	}

	public FairUser(Mcv mcv) : base(mcv)
	{
	}

	public override User Clone()
	{
		var a = base.Clone() as FairUser;

		a.Authors				= Authors;
		a.ModeratedSites		= ModeratedSites;
		a.Sites					= Sites;
		a.Reviews				= Reviews;
		a.Avatar				= Avatar;
		a.FavoriteSites			= FavoriteSites;
		a.Approvals				= Approvals;
		a.Rejections			= Rejections;

		return a;
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(Authors);
		writer.Write(ModeratedSites);
		writer.Write(Sites);
		writer.Write(FavoriteSites);
		writer.Write(Reviews);
		writer.WriteBytes(Avatar);
		writer.Write7BitEncodedInt(Approvals);
		writer.Write7BitEncodedInt(Rejections);
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		Authors					= reader.ReadArray<AutoId>();
		ModeratedSites			= reader.ReadArray<AutoId>();
		Sites					= reader.ReadArray<AutoId>();
		FavoriteSites			= reader.ReadArray<AutoId>();
		Reviews					= reader.ReadArray<AutoId>();
		Avatar					= reader.ReadBytes();
		Approvals				= reader.Read7BitEncodedInt();
		Rejections				= reader.Read7BitEncodedInt();
	}
}
