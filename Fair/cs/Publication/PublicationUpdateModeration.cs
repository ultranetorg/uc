using System.Text;

namespace Uccs.Fair;

public class PublicationUpdateModeration : VotableOperation
{
	public AutoId						Publication { get; set; }
	public ProductFieldVersionReference	Change { get; set; }
	public bool							Resolution { get; set; }

	public override bool				IsValid(McvNet net) => true;
	public override string				Explanation => $"{Publication}, {Change}, {Resolution}";

	public PublicationUpdateModeration()
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
		var o = other as PublicationUpdateModeration;

		return o.Publication == Publication && o.Change.Name == Change.Name;
	}
	
	public override bool ValidProposal(FairExecution execution)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return false;

		var r = execution.FindProduct(p.Product);

		if(!r.Fields.Any(i => i.Name == Change.Name && i.Versions.Any(i => i.Version == Change.Version)))
		{
			Error = NotFound;
			return false;
		}

		if(!p.Changes.Any(i => i.Name == Change.Name && i.Version == Change.Version))
		{
			Error = NotFound;
			return false;
		}

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

	 		if(s.ChangePolicies[FairOperationClass.PublicationUpdateModeration] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}	 	

		var p = execution.AffectPublication(Publication);
		var a = execution.AffectAuthor(execution.FindProduct(p.Product).Author);
		var c = p.Changes.First(i => i.Name == Change.Name && i.Version == Change.Version);
 
		if(Resolution == true)
		{
			var r = execution.AffectProduct(p.Product);
			var f = r.Fields.First(i => i.Name == Change.Name);
				
			p.Changes = [..p.Changes.Where(i => i != c)];
								
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
					execution.PublicationTitles.Deindex(execution.FindCategory(p.Category).Site, p, r.Get(prev).AsUtf8);
			}
	
			/// increase refs in product
					
			var v = f.Versions.First(i => i.Version == Change.Version);
	
			f = new ProductField {Name = f.Name, 
								  Versions = [..f.Versions.Where(i => i.Version != v.Version), new ProductFieldVersion(v.Version, v.Value, v.Refs + 1)]};
	
			r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];

			if(f.Name == ProductField.Title)
				execution.PublicationTitles.Index(execution.FindCategory(p.Category).Site, p.Id, v.AsUtf8);

			PayForModeration(execution, p, a);
		}
		else
		{	
			p.Changes = [..p.Changes.Where(i => i != c)];

			PayForModeration(execution, p, a);
		}
	}
}
