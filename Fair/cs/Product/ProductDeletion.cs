namespace Uccs.Fair
{
	public class ProductDeletion : FairOperation
	{
		public new ProductId		Id { get; set; }

		public override bool		IsValid(Mcv mcv) => true;
		public override string		Description => $"{Id}";

		public ProductDeletion()
		{
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Id = reader.Read<ProductId>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Id);
		}

		public override void Execute(FairMcv mcv, FairRound round)
		{
			if(RequireSignerProduct(round, Id, out var a, out var r) == false)
				return;

			a = round.AffectPublisher(Id.PublisherId);
			a.DeleteProduct(r);

			Free(a, r.Length);
		}
	}
}