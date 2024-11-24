namespace Uccs.Fair
{
	public class ProductCreation : FairOperation
	{
		public EntityId				Publisher { get; set; }
		public ProductChanges		Changes { get; set; }
		public byte[]				Data { get; set; }

		public override bool		IsValid(Mcv mcv) => !Changes.HasFlag(ProductChanges.SetData) || (Data.Length <= Product.DataLengthMax);
		public override string		Description => $"{Publisher}, [{Changes}]{(Data == null ? null : ", Data=" + Data)}";

		public ProductCreation()
		{
		}

		public ProductCreation(EntityId publisher, byte[] data)
		{
			Publisher = publisher;
			Data = data;

			if(Data != null)	Changes |= ProductChanges.SetData;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Publisher	= reader.Read<EntityId>();
			Changes	= (ProductChanges)reader.ReadByte();
			Data = reader.ReadBytes();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Publisher);
			writer.Write((byte)Changes);
			writer.WriteBytes(Data);
		}

		public override void Execute(FairMcv mcv, FairRound round)
		{
			if(RequireSignerPublisher(round, Publisher, out var a) == false)
				return;

			a = round.AffectPublisher(Publisher);
			var r = a.AffectProduct(null);

			if(Changes.HasFlag(ProductChanges.SetData))
			{
				r.Data		= Data;
				r.Updated	= round.ConsensusTime;
			}

			Allocate(round, a, r.Length);
		}
	}
}