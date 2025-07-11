using System.Text;

namespace Uccs.Fair;

public class PublicationUpdation : VotableOperation
{
	public AutoId						Publication { get; set; }
	public ProductFieldVersionReference	Change { get; set; }

	public override bool				IsValid(McvNet net) => true;
	public override string				Explanation => $"{Publication}, {{{Change}}}";

	public PublicationUpdation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
		Change		= reader.Read<ProductFieldVersionReference>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Change);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationUpdation;

		return o.Publication == Publication && o.Change.Field == Change.Field;
	}
	
	public override bool ValidateProposal(FairExecution execution)
	{
		if(!PublicationExists(execution, Publication, out var p, out _))
			return false;

		var r = execution.Products.Find(p.Product);

		if(!r.Fields.Any(i => i.Name == Change.Field && i.Versions.Any(i => i.Version == Change.Version)))
		{
			Error = NotFound;
			return false;
		}

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var p = execution.Publications.Affect(Publication);
		var a = execution.Authors.Affect(execution.Products.Find(p.Product).Author);
		var r = execution.Products.Affect(p.Product);

		var f = r.Fields.First(i => i.Name == Change.Field);
								
		var prev = p.Fields.FirstOrDefault(i => i.Field == Change.Field);
	
		if(prev == null)	/// new field
			p.Fields = [..p.Fields, Change];
		else				/// replace version
		{
			p.Fields = [..p.Fields.Where(i => i.Field != Change.Field), Change];
		
			/// decrease refs in product
			var x = f.Versions.First(i => i.Version == prev.Version);
	
			f = new ProductField {Name = f.Name, 
								  Versions = [..f.Versions.Where(i => i.Version != x.Version), new ProductFieldVersion(x.Version, x.Value, x.Refs - 1)]};
		
			r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];

			if(f.Name == ProductFieldName.Title)
				execution.PublicationTitles.Deindex(p.Site, r.Get(prev).AsUtf8);
		}
	
		/// increase refs in product
					
		var v = f.Versions.First(i => i.Version == Change.Version);
	
		f = new ProductField {Name = f.Name, 
							  Versions = [..f.Versions.Where(i => i.Version != v.Version), new ProductFieldVersion(v.Version, v.Value, v.Refs + 1)]};
	
		r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];

		if(f.Name == ProductFieldName.Title)
			execution.PublicationTitles.Index(p.Site, p.Id, v.AsUtf8);

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{ 
			var s = execution.Sites.Find(p.Site);

			RewardForModeration(execution, a, s);
		}
	}
}
