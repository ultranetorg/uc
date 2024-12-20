namespace Uccs.Fair;

public class ProductCreation : FairOperation
{
	public EntityId				Author { get; set; }
	public ProductChanges		Changes { get; set; }
	public byte[]				Data { get; set; }

	public override bool		IsValid(Mcv mcv) => !Changes.HasFlag(ProductChanges.SetData) || (Data.Length <= Product.DataLengthMax);
	public override string		Description => $"{Author}, [{Changes}]{(Data == null ? null : ", Data=" + Data)}";

	public ProductCreation()
	{
	}

	public ProductCreation(EntityId author, byte[] data)
	{
		Author	= author;
		Data	= data;

		if(Data != null)	Changes |= ProductChanges.SetData;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Author	= reader.Read<EntityId>();
		Changes	= (ProductChanges)reader.ReadByte();
		Data	= reader.ReadBytes();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Author);
		writer.Write((byte)Changes);
		writer.WriteBytes(Data);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireSignerAuthor(round, Author, out var d) == false)
			return;

		var r = round.AffectProduct(d);

		if(Changes.HasFlag(ProductChanges.SetData))
		{
			r.Data		= Data;
			//r.Flags		|= ProductFlags.Data;
			r.Updated	= round.ConsensusTime;
		}

		d = round.AffectAuthor(d.Id);
		Allocate(round, d, r.Length);
	}
}