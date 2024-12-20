namespace Uccs.Fair
{
	public class ProductUpdation : FairOperation
	{
		public new ProductId		Id { get; set; }
		public ProductChanges		Changes	{ get; set; }
		public byte[]				Data { get; set; }

		public override bool		IsValid(Mcv mcv) => !Changes.HasFlag(ProductChanges.SetData) || Data.Length <= Product.DataLengthMax;
		public override string		Description => $"{Id}, [{Changes}], {(Data == null ? null : $", Data={{{Data}}}")}";

		public ProductUpdation()
		{
		}

		public ProductUpdation(ProductId resource)
		{
			Id = resource;
		}

		public void Change(byte[] data)
		{
			Data = data;
			Changes = ProductChanges.SetData;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Id	= reader.Read<ProductId>();
			Changes	= (ProductChanges)reader.ReadByte();
			
			if(Changes.HasFlag(ProductChanges.SetData))	Data = reader.ReadBytes();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write((byte)Changes);

			if(Changes.HasFlag(ProductChanges.SetData))	writer.WriteBytes(Data);
		}

		public override void Execute(FairMcv mcv, FairRound round)
		{
			if(RequireSignerProduct(round, Id, out var p, out var _) == false)
				return;

			var r = round.AffectProduct(Id);

			if(Changes.HasFlag(ProductChanges.SetData))
			{
				//r.Flags		|= ProductFlags.Data;
				r.Data		= Data;
				r.Updated	= round.ConsensusTime;
	
				Allocate(round, p, r.Data.Length);
			}
		}
	}
}