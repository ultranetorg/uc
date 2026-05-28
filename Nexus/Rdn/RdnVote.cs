namespace Uccs.Rdn;

public class RdnVote : Vote
{
	//public ForeignResult[]		Emissions = {};

	public RdnVote(Mcv mcv) : base(mcv)
	{
	}

	protected override void WritePayload(Writer writer)
	{
		base.WritePayload(writer);

		//writer.Write(Emissions);
	}

	protected override void ReadPayload(Reader reader)
	{
		base.ReadPayload(reader);

		//Emissions	= reader.ReadArray<ForeignResult>();
	}
}
