using System.Text;

namespace Uccs.Fair;

public class PublicationUpdation : VotableOperation
{
	public AutoId						Publication { get; set; }
	public ProductFieldVersionReference	Change { get; set; }
	public bool							Resolution { get; set; }

	public override bool				IsValid(McvNet net) => true;
	public override string				Explanation => $"{Publication}, {Change}, {Resolution}";

	public PublicationUpdation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
		Change		= reader.Read<ProductFieldVersionReference>();
		Resolution	= reader.ReadBoolean();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Change);
		writer.Write(Resolution);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationUpdation;

		return o.Publication == Publication && o.Change.Name == Change.Name;
	}
	
	public override bool ValidProposal(FairExecution execution)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return false;

		var r = execution.Products.Find(p.Product);

		if(!r.Fields.Any(i => i.Name == Change.Name && i.Versions.Any(i => i.Version == Change.Version)))
		{
			Error = NotFound;
			return false;
		}

// 		if(!p.Changes.Any(i => i.Name == Change.Name && i.Version == Change.Version))
// 		{
// 			Error = NotFound;
// 			return false;
// 		}

		return true;
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
			if(!RequirePublicationModeratorAccess(execution, Publication, Signer, out var _, out var s))
				return;

	 		if(s.ChangePolicies[FairOperationClass.PublicationUpdation] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}	 	

		var p = execution.Publications.Affect(Publication);
		var a = execution.Authors.Affect(execution.Products.Find(p.Product).Author);
		
		if(Resolution == true)
		{
			var r = execution.Products.Affect(p.Product);
			var f = r.Fields.First(i => i.Name == Change.Name);
								
			var prev = p.Fields.FirstOrDefault(i => i.Name == Change.Name);
	
			if(prev == null)	/// new field
				p.Fields = [..p.Fields, Change];
			else				/// replace version
			{
				p.Fields = [..p.Fields.Where(i => i.Name != Change.Name), Change];
		
				/// decrease refs in product
				var x = f.Versions.First(i => i.Version == prev.Version);
	
				f = new ProductField{Name = f.Name, 
									 Versions = [..f.Versions.Where(i => i.Version != x.Version), new ProductFieldVersion(x.Version, x.Value, x.Refs - 1)]};
		
				r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];

				if(f.Name == ProductField.Title)
					execution.PublicationTitles.Deindex(p.Site, r.Get(prev).AsUtf8);
			}
	
			/// increase refs in product
					
			var v = f.Versions.First(i => i.Version == Change.Version);
	
			f = new ProductField {Name = f.Name, 
								  Versions = [..f.Versions.Where(i => i.Version != v.Version), new ProductFieldVersion(v.Version, v.Value, v.Refs + 1)]};
	
			r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];

			if(f.Name == ProductField.Title)
				execution.PublicationTitles.Index(p.Site, p.Id, v.AsUtf8);
		}
	
		PayEnergyForModeration(execution, p, a);
	}
}
