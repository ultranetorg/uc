using System.Collections.Immutable;

namespace Uccs.Fair;

public class FairUser : User
{
	public ImmutableSortedSet<AutoId>	Authors  { get; set; }
	public ImmutableSortedSet<AutoId>	ModeratedStores  { get; set; }
	public ImmutableSortedSet<AutoId>	Stores  { get; set; }
	public ImmutableSortedSet<AutoId>	Reviews  { get; set; }
	public ImmutableSortedSet<AutoId>	FavoriteStores  { get; set; }
	public int							Approvals  { get; set; }
	public int							Rejections  { get; set; }
	public byte[]						Avatar  { get; set; }

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
		a.ModeratedStores		= ModeratedStores;
		a.Stores				= Stores;
		a.Reviews				= Reviews;
		a.Avatar				= Avatar;
		a.FavoriteStores		= FavoriteStores;
		a.Approvals				= Approvals;
		a.Rejections			= Rejections;

		return a;
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);

		writer.Write(Authors);
		writer.Write(ModeratedStores);
		writer.Write(Stores);
		writer.Write(Reviews);
		writer.WriteBytes(Avatar);
		writer.Write7BitEncodedInt(Approvals);
		writer.Write7BitEncodedInt(Rejections);
		writer.Write(FavoriteStores);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);

		Authors					= reader.ReadImmutableSortedSet<AutoId>();
		ModeratedStores			= reader.ReadImmutableSortedSet<AutoId>();
		Stores					= reader.ReadImmutableSortedSet<AutoId>();
		Reviews					= reader.ReadImmutableSortedSet<AutoId>();
		Avatar					= reader.ReadBytes();
		Approvals				= reader.Read7BitEncodedInt();
		Rejections				= reader.Read7BitEncodedInt();
		FavoriteStores			= reader.ReadImmutableSortedSet<AutoId>();
	}
}
