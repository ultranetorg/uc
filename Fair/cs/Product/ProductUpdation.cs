using System.Text;

namespace Uccs.Fair;

public class ProductUpdation : FairOperation
{
	public AutoId				Product { get; set; }
	public FieldValue[]			Fields	{ get; set; }

	public override string		Explanation => $"Product={Product}, Fields={Fields.Length}";


	public ProductUpdation()
	{
	}

	public ProductUpdation(AutoId id)
	{
		Product = id;
	}
	
	public override bool IsValid(McvNet net)
	{
		if(Fields.Any(i => !i.IsValid(net)))
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Product	= reader.Read<AutoId>();
		Fields	= reader.ReadArray<FieldValue>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.Write(Fields);
	}

	public override void Execute(FairExecution execution)
	{
		if(CanAccessProduct(execution, Product, out var a, out var r, out Error) == false)
			return;

		a = execution.Authors.Affect(a.Id);
		r = execution.Products.Affect(Product);

		var v = new ProductVersion {Fields = Fields};

		var d = Uccs.Fair.Product.FindDeclaration(r.Type);

		var f = v.Find(d, (f, i) =>	{
										if(f.Type == FieldType.FileId)
										{	
											var x = execution.Files.Find(i.AsAutoId);

											if(x == null)
												return true;
 										}

										return false;
									});
		if(f != null)
		{
			Error = NotFound;
			return;
		}

		if(r.Versions.Any() && r.Versions.Last().Refs == 0)
		{
			execution.Free(a, a, r.Versions.Last().Size);

			v.Id = r.Versions.Last().Id;
			r.Versions = [..r.Versions[..^1], v];
		}
		else
		{	
			v.Id = r.Versions.Length == 0 ? 0 : r.Versions.Last().Id + 1;
			r.Versions = [..r.Versions, v];
		}

		execution.Allocate(a, a, r.Versions.Last().Size);


		foreach(var p in r.Publications)
		{
			var s = execution.Sites.Affect(execution.Publications.Find(p).Site);
				
			if(!s.ChangedPublications.Contains(p))
				s.ChangedPublications = [..s.ChangedPublications, p];
		}

		r.Updated = execution.Time;

		execution.PayCycleEnergy(a);
	}
}