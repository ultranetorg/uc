namespace Uccs.Fair;

public class PublicationCreation : VotableOperation
{
	public AutoId				Product { get; set; }

	public override bool		IsValid(McvNet net) => Product != null;
	public override string		Explanation => $"Product={Product}";

	public PublicationCreation()
	{
	}

	public PublicationCreation(AutoId product)
	{
		Product = product;
	}

	public override void Read(BinaryReader reader)
	{
		Product = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Product);
	}
	
	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationCreation;

		return o.Site == Site && o.Product == Product;
	}
	
	public override bool ValidateProposal(FairExecution execution)
	{
		if(ProductExists(execution, Product, out var a, out var r, out _) == false)
			return false;

		if(r.Publications.Any(i => execution.Publications.Find(i).Site == Site.Id))
			return false;

		if(Site.UnpublishedPublications.Any(i => execution.Publications.Find(i).Product == Product))
			return false;

		return true;
	}
	
	public override void Execute(FairExecution execution)
	{
//		if(!ValidateProposal(execution))
//			return;
//
//		if(!dispute)
//	 	{
//			if(!CanModerate(execution, Site, out var x))
//				return;
//
//	 		if(x.ChangePolicies[FairOperationClass.PublicationCreation] != ChangePolicy.AnyModerator)
//	 		{
//		 		Error = Denied;
//		 		return;
//	 		}
//		}

		var r = execution.Products.Affect(Product);
		var s = execution.Sites.Affect(Site.Id);

		var p = execution.Publications.Create(s);

//		var p =	execution.Publications.Affect(Publication);
//
//		if(p.Category == null)
//		{
//			Error = CategoryNotSet;
//			return;
//		}

//		s = execution.Sites.Affect(p.Site);
		
		
		if(CanAccessAuthor(execution, r.Author, out _, out _))
		{ 
			var a = execution.Authors.Affect(r.Author);

			p.Flags = PublicationFlags.ApprovedByAuthor;

			//a.Energy	-= s.ExetranalRequestFee;
			//s.Energy	+= s.ExetranalRequestFee;
			//
			//Allocate(execution, a, s, execution.Net.EntityLength); /// author-spender , site-consumer
			//PayEnergyByAuthor(execution, a.Id);
		}
//		else if(CanAccessSite(execution, s.Id))
//		{	
//			Allocate(execution, s, s, execution.Net.EntityLength);
//			PayEnergyBySite(execution, s.Id);
//		}

		p.Site		= s.Id;
		p.Product	= r.Id;
		p.Creator	= Signer.Id;
	
		r.Publications = [..r.Publications, p.Id];

		s.PublicationsCount++;

		//var c = execution.Categories.Find(p.Category);
		//var r = execution.Products.Find(p.Product);
		//var a = execution.Authors.Find();

		if(!s.Authors.Contains(r.Author))
		{
			var a = execution.Authors.Affect(r.Author);
			//s = execution.Sites.Affect(s.Id);

			s.Authors = [..s.Authors, a.Id];
			a.Sites = [..a.Sites, s.Id];
		}

		s.UnpublishedPublications = [..s.UnpublishedPublications, p.Id];

		//if(!c.Publications.Contains(p.Id))
		//{
		//	c = execution.Categories.Affect(c.Id);
		//	c.Publications = [..c.Publications, p.Id];
		//
		//	s.PublicationsCount++;
		//}

		var tr = p.Fields.FirstOrDefault(i => i.Field == ProductFieldName.Title);
			
		if(tr != null)
			execution.PublicationTitles.Index(s.Id, p.Id, r.Get(tr).AsUtf8);

		execution.Allocate(s, s, execution.Net.EntityLength);
	}
}