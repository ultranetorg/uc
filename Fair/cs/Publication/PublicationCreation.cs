namespace Uccs.Fair;

public class PublicationCreation : FairOperation
{
	public EntityId					Product { get; set; }
	public EntityId					Category { get; set; }
	//public ProductFieldVersionId[]	Fields { get; set; }

	public override bool		IsValid(Mcv mcv) => Product != null && Category != null;
	public override string		Description => $"Product={Product} Category={Category}";

	public override void ReadConfirmed(BinaryReader reader)
	{
		Product = reader.Read<EntityId>();
		Category= reader.Read<EntityId>();
		//Fields	= reader.ReadArray<ProductFieldVersionId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.Write(Category);
		//writer.Write(Fields);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireProduct(round, Product, out var a, out var pr))
			return;

		if(!RequireCategory(round, Category, out var c))
			return;
					
		var p = round.CreatePublication(mcv.Sites.Find(c.Site, round.Id));

		if(Signer.Id == a.Id)
			p.Status = PublicationStatus.RequestedByAuthor;
		else if(mcv.Sites.Find(c.Site, mcv.LastConfirmedRound.Id)?.Moderators.Contains(Signer.Id) ?? false)
			p.Status = PublicationStatus.ProposedBySite;
		else
		{
			Error = Denied;
			return;
		}

		p.Product	= Product;
		p.Category	= Category;
		p.Creator	= Signer.Id;

		var r = round.AffectProduct(Product);
		r.Publications = [..r.Publications, p.Id];

		c = round.AffectCategory(c.Id);
		c.Publications = [..c.Publications, p.Id];

// 		foreach(var i in Fields)
// 		{
// 			var f = r.Fields.FirstOrDefault(j => j.Name == i.Name);
// 
// 			if(f == null)
// 			{
// 				Error = NotFound;
// 				return;
// 			}
// 
// 			var v = f.Versions.FirstOrDefault(j => j.Id == i.Id);
// 
// 			if(v == null)
// 			{
// 				Error = NotFound;
// 				return;
// 			}
// 
// 			///p.Fields = [..p.Fields, i];
// 
// 
// 
// 			var y = rf.Versions.First(i => i.Id == v.Id);
// 	
// 			rf = new ProductField {Name = rf.Name, 
// 									Versions = [..rf.Versions.Where(i => i.Id != y.Id), new ProductFieldVersion {Id = y.Id, Value = y.Value, Refs = y.Refs + 1}]};
// 	
// 			r.Fields = [..r.Fields.Where(i => i.Name != v.Name), rf];
// 
// 		}
	}
}