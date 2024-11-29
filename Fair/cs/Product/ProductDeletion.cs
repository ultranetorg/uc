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
			if(RequireSignerProduct(round, Id, out var p, out var r) == false)
				return;

			p = round.AffectPublisher(Id.PublisherId);
			var a = round.AffectAssortment(Id.PublisherId);
			
			a.DeleteProduct(r);

			Free(p, r.Length);
		}
	}
}