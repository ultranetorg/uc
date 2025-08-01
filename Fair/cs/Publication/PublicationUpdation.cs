using System.Text;

namespace Uccs.Fair;

public class PublicationUpdation : VotableOperation
{
	public AutoId				Publication { get; set; }
	public int					Version { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Publication={Publication}, Version={Version}";

	public PublicationUpdation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
		Version		= reader.Read7BitEncodedInt();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write7BitEncodedInt(Version);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationUpdation;

		return o.Publication == Publication;
	}
	
	public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!PublicationExists(execution, Publication, out var p, out error))
			return false;

		var r = execution.Products.Find(p.Product);

		if(!r.Versions.Any(i => i.Id == Version))
		{
			error = NotFound;
			return false;
		}

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var p = execution.Publications.Affect(Publication);
		var a = execution.Authors.Affect(execution.Products.Find(p.Product).Author);
		var r = execution.Products.Affect(p.Product);

		var v = r.Versions.First(i => i.Id == Version);
		var prev = r.Versions.FirstOrDefault(i => i.Id == p.ProductVersion);

		var d = Product.FindDeclaration(r.Type);
	
		if(prev == null)	/// new field
			p.ProductVersion = Version;
		else				/// replace version
		{
			//p.Fields = [..p.Fields.Where(i => i.Field != Change.Field), Change];
		
			/// decrease refs in product
			//var x = f.Versions.First(i => i.Version == prev.Version);

			v.ForEach(d, (f, i) =>	{
										if(f.Type == FieldType.FileId)
										{
											var x = execution.Files.Affect(i.AsAutoId);
											x.Refs--;
										}
									});
	
			var x = new ProductVersion
					{
						Id		= prev.Id, 
						Refs	= prev.Refs - 1,
						Fields	= prev.Fields
					};
		
			r.Versions = [..r.Versions.Where(i => i.Id != prev.Id), x];

			var xtitle = prev.Fields.FirstOrDefault(i => i.Name == Token.Title);
			
			if(xtitle != null)
				execution.PublicationTitles.Deindex(p.Site, xtitle.AsUtf8);
		}
	
		/// increase refs in product
					
		//var v = f.Versions.First(i => i.Version == Change.Version);
	
		var y = new ProductVersion {Id		= v.Id, 
									Refs	= v.Refs + 1,
									Fields	= v.Fields};
	
		r.Versions = [..r.Versions.Where(i => i.Id != v.Id), y];

		v.ForEach(d, (f, i) =>	{
									if(f.Type == FieldType.FileId)
									{
										var x = execution.Files.Affect(i.AsAutoId);
										x.Refs++;
									}
								});

		var title = prev.Fields.FirstOrDefault(i => i.Name == Token.Title);

		if(title != null)
			execution.PublicationTitles.Index(p.Site, p.Id, title.AsUtf8);

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{ 
			var s = execution.Sites.Find(p.Site);

			RewardForModeration(execution, a, s);
		}
	}
}
