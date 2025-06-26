using System.Text;

namespace Uccs.Fair;

public class ProductUpdation : FairOperation
{
	public AutoId				Product { get; set; }
	public string				Name	{ get; set; }
	public byte[]				Value	{ get; set; }
	public override string		Explanation => $"{Product}, {Name}, {Value}";

	public override bool		IsValid(McvNet net) => Value.Length <= ProductField.ValueLengthMaximum;

	public ProductUpdation()
	{
	}

	public ProductUpdation(AutoId id)
	{
		Product = id;
	}

	//public void Change(string change, string data)
	//{
	//	Changes ??= [];
	//
	//	var f = Changes.FirstOrDefault(i => i.Name == change);
	//
	//	if(f == null)
	//	{
	//		Changes = [..Changes, new (change, data)];
	//	}
	//	else
	//		f.Value  = data;
	//}

	public override void Read(BinaryReader reader)
	{
		Product	= reader.Read<AutoId>();
		Name		= reader.ReadUtf8();
		Value		= reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.WriteUtf8(Name);
		writer.WriteBytes(Value);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(RequireProductAccess(execution, Product, out var a, out var r) == false)
			return;

		a = execution.Authors.Affect(a.Id);
		r = execution.Products.Affect(Product);

		var f = r.Fields.FirstOrDefault(j => j.Name == Name);

		if(f == null)
		{	
			f = new ProductField {Name = Name, Versions = [new ProductFieldVersion {Value = Value, Version = 0}]};
			r.Fields = [..r.Fields, f];
		}
		else
		{
			f = new ProductField {Name = f.Name, Versions = f.Versions};
			r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];

			if(f.Versions.Last().Refs == 0)
			{
				Free(execution, Signer, a, f.Versions.Last().Value.Length);

				f.Versions = [..f.Versions[..^1], new ProductFieldVersion {Value = Value, Version = f.Versions.Last().Version}];
			}
			else
				f.Versions = [..f.Versions, new ProductFieldVersion {Value = Value, Version = f.Versions.Length}];
		}

		Allocate(execution, Signer, a, Value.Length);

		foreach(var p in r.Publications)
		{
			var s = execution.Sites.Affect(execution.Publications.Find(p).Site);
				
			if(!s.PendingPublications.Contains(p))
			{
				s.PendingPublications = [..s.PendingPublications, p];
			} 
		}

		r.Updated = execution.Time;
	}
}