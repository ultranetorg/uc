namespace Uccs.Fair;

public class PublicationUpdateModeration : PublicationUpdation
{
	public EntityId						Publication { get; set; }
	public ProductFieldVersionReference	Change { get; set; }
	public bool							Resolution { get; set; }

	public override bool				IsValid(Mcv mcv) => true;
	public override string				Description => $"{Publication}, {Change}, {Resolution}";

	public PublicationUpdateModeration()
	{
	}
	
	public override bool ValidProposal(FairMcv mcv, FairRound round, Site site)
	{
		if(!RequirePublication(round, Publication, out var p))
			return false;

		var r = round.FindProduct(p.Product);

		return r.Fields.Any(i => i.Name == Change.Name && i.Versions.Any(i => i.Version == Change.Version));
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationUpdateModeration;

		return o.Publication == Publication && o.Change.Name == Change.Name;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Change		= reader.Read<ProductFieldVersionReference>();
		Resolution	= reader.ReadBoolean();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Change);
		writer.Write(Resolution);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p, out var s))
			return;

		if(s.ChangePolicies[FairOperationClass.PublicationUpdateModeration] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}

		Execute(mcv, round, s);
	}

	public override void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
		var p = round.AffectPublication(Publication);
		var a = round.AffectAuthor(round.FindProduct(p.Product).Author);
 
		if(Resolution == true)
		{
			var r = round.AffectProduct(p.Product);

			var f = r.Fields.First(i => i.Name == Change.Name);

			var c = p.Changes.FirstOrDefault(i => i.Name == Change.Name && i.Version == Change.Version);

			if(c == null)
			{
				Error = NotFound;
				return;
			}
				
			p.Changes = [..p.Changes.Where(i => i != c)];
								
			if(f != null)
			{
				var prev = p.Fields.FirstOrDefault(i => i.Name == Change.Name);
	
				if(prev == null)	/// new field
					p.Fields = [..p.Fields, Change];
				else			/// replace version
					p.Fields = [..p.Fields.Where(i => i.Name != Change.Name), Change];
		
				if(prev != null) /// if previously used then decrease refs in product
				{
					var x = f.Versions.First(i => i.Version == prev.Version);
	
					f = new ProductField {Name = f.Name, 
											Versions = [..f.Versions.Where(i => i.Version != x.Version), new ProductFieldVersion(x.Version, x.Value, x.Refs - 1)]};
		
					r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];
				}
	
				/// increase refs in product
					
				var y = f.Versions.First(i => i.Version == Change.Version);
	
				f = new ProductField {Name = f.Name, 
										Versions = [..f.Versions.Where(i => i.Version != y.Version), new ProductFieldVersion(y.Version, y.Value, y.Refs + 1)]};
	
				r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];
			} 
			else /// a field is deleted from product
			{
				p.Fields = [..p.Fields.Where(i => i.Name != Change.Name)];
			}

			Pay(round, p, a);
		}
		else
		{	
			var c = p.Changes.FirstOrDefault(i => i.Name == Change.Name && i.Version == Change.Version);

			if(c == null)
			{
				Error = NotFound;
				return;
			}
				
			p.Changes = [..p.Changes.Where(i => i != c)];

			Pay(round, p, a);
		}
	}
}
