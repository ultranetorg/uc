namespace Uccs.Rdn;

public class RdnVote : Vote
{
	//public ForeignResult[]		Emissions = {};

	public RdnVote(Mcv mcv) : base(mcv)
	{
	}

	protected override void WritePayload(BinaryWriter writer)
	{
		base.WritePayload(writer);

		//writer.Write(Emissions);
	}

	protected override void ReadPayload(BinaryReader reader)
	{
		base.ReadPayload(reader);

		//Emissions	= reader.ReadArray<ForeignResult>();
	}
}
