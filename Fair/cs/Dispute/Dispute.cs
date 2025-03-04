namespace Uccs.Fair;

// public class Security : IBinarySerializable
// {
// 	public OrderedDictionary<TopicChange, Actor[]>	Permissions;
// 
// 	public static Security Parse(string v)
// 	{
// 		return new Security {Permissions = new (new Xon(v).Nodes.ToDictionary(i => Enum.Parse<TopicChange>(i.Name), 
// 																			  i => i.Get<string>().Split(',').Select(i => Enum.Parse<Actor>(i)).ToArray()))};
// 	}
// 
// 	public void Read(BinaryReader reader)
// 	{
// 		Permissions = reader.ReadOrderedDictionary(() => (TopicChange)reader.Read7BitEncodedInt64(),
// 												   () => reader.ReadArray(() => (Actor)reader.Read7BitEncodedInt64()));
// 	}
// 
// 	public void Write(BinaryWriter writer)
// 	{
// 		writer.Write(Permissions, i =>	{ 
// 											writer.Write7BitEncodedInt64((long)i.Key); 
// 											writer.Write(i.Value, i => writer.Write7BitEncodedInt64((long)i)); 
// 										});
// 	}
// 	
// 	public Security Clone()
// 	{
// 		return new Security {Permissions = new (Permissions)};
// 	}
// }

public enum ProposalChange : byte
{
	None, Moderators, ModeratorElectionPolicy, Authors
}

public class Proposal : IBinarySerializable
{
	public ProposalChange	Change { get; set; }
	public object			First { get; set; }
	public object			Second { get; set; }
	public string			Text { get; set; }

	public bool Overlaps(Proposal other)
	{
		if(Change != other.Change)
			return false;

		switch(Change)
		{
			case ProposalChange.Authors :
			case ProposalChange.Moderators :
			{	
				var f = other.First as EntityId[];
				var s = other.Second as EntityId[];
				
				foreach(var i in First as EntityId[])
					if(f.Contains(i) || s.Contains(i))
						return true;

				foreach(var i in Second as EntityId[])
					if(f.Contains(i) || s.Contains(i))
						return true;
				
				return false;
			}

			case ProposalChange.ModeratorElectionPolicy :
				return true;
		}

		throw new IntegrityException();
	}
	
	public void Read(BinaryReader reader)
	{
		Text	= reader.ReadUtf8();
		Change	= reader.ReadEnum<ProposalChange>();

		switch(Change)
		{
			case ProposalChange.Authors :
			case ProposalChange.Moderators :
				First = reader.ReadArray<EntityId>();
				Second = reader.ReadArray<EntityId>();
				break;

			case ProposalChange.ModeratorElectionPolicy :
				First = reader.ReadEnum<ElectionPolicy>();
				break;
		}
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Text);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case ProposalChange.Authors :
			case ProposalChange.Moderators :
				writer.Write(First as EntityId[]);
				writer.Write(Second as EntityId[]);
				break;

			case ProposalChange.ModeratorElectionPolicy :
				writer.WriteEnum((ElectionPolicy)First);
				break;
		}
	}

	public bool Valid(SiteEntry site, FairRound round)
	{
		switch(Change)
		{
			case ProposalChange.Authors :
			{	
				foreach(var i in First as EntityId[])
					if(site.Authors.Contains(i))
						return false;

				foreach(var i in Second as EntityId[])
					if(!site.Authors.Contains(i))
						return false;
				break;
			}

			case ProposalChange.Moderators :
			{	
				foreach(var i in First as EntityId[])
					if(site.Moderators.Contains(i))
						return false;

				foreach(var i in Second as EntityId[])
					if(!site.Moderators.Contains(i))
						return false;
				break;
			}

			case ProposalChange.ModeratorElectionPolicy :
				return (ElectionPolicy)First != site.ModeratorElectionPolicy;
		}

		return true;
	}

	public void Execute(EntityId site, FairRound round)
	{
		var s = round.AffectSite(site);

		switch(Change)
		{
			case ProposalChange.Authors : 
			{	
				s.Authors = [..s.Authors, ..First as EntityId[]];

				foreach(var i in Second as EntityId[])
					s.Authors = s.Authors.Remove(i);
	
				break;
			}

			case ProposalChange.ModeratorElectionPolicy : 
			{
				s.ModeratorElectionPolicy = (ElectionPolicy)First;
				break;
			}

			case ProposalChange.Moderators : 
			{	
				s.Moderators = [..s.Moderators, ..First as EntityId[]];

				foreach(var i in Second as EntityId[])
					s.Moderators = s.Moderators.Remove(i);
	
				break;
			}
		}
	}
}

public enum DisputeFlags : byte
{
	Resolved	= 0b0001,
	Referendum	= 0b0010,
}

public class Dispute : IBinarySerializable
{
	public EntityId			Id { get; set; }
	public EntityId			Site { get; set; }
	public DisputeFlags		Flags { get; set; }
	public Proposal			Proposal { get; set; }
	public EntityId[]		Pros { get; set; }
	public EntityId[]		Cons { get; set; }
	public Time				Expirtaion { get; set; }

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Site		= reader.Read<EntityId>();
		Flags		= reader.ReadEnum<DisputeFlags>();
		Proposal	= reader.Read<Proposal>();
		Pros		= reader.ReadArray<EntityId>();
		Cons		= reader.ReadArray<EntityId>();
		Expirtaion	= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.WriteEnum(Flags);
		writer.Write(Proposal);
		writer.Write(Pros);
		writer.Write(Cons);
		writer.Write(Expirtaion);
	}
}
