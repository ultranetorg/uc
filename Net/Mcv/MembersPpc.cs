namespace Uccs.Net;

public class MembersPpc : McvPpc<MembersPpr>, IBinarySerializable
{
	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			if(Mcv.NextVotingRound.Senders.Count() == 0)
				throw new EntityException(EntityError.NoMembers);

			return new MembersPpr {Members = Mcv.NextVotingRound.Senders.ToArray()};
		}
	}

	public void Read(Reader reader)
	{
	}

	public void Write(Writer writer)
	{
	}
}

public class MembersPpr : Result, IBinarySerializable 
{
	public Generator[] Members { get; set; }

	public void Read(Reader reader)
	{
		Members = reader.ReadArray(() => {var g = new Generator(); g.ReadBase(reader); return g;});
	}

	public void Write(Writer writer)
	{
		writer.Write(Members, i => i.WriteBase(writer));
	}
}
