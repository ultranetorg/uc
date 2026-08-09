namespace Uccs.Net;

public class MembersPpc : McvPpc<MembersPpr>
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

	public override void Read(Reader reader)
	{
	}

	public override void Write(Writer writer)
	{
	}
}

public class MembersPpr : Result 
{
	public Member[] Members { get; set; }

	public override void Read(Reader reader)
	{
		Members = reader.ReadArray(() => {var g = new Member(); g.ReadBase(reader); return g;});
	}

	public override void Write(Writer writer)
	{
		writer.Write(Members, i => i.WriteBase(writer));
	}
}
