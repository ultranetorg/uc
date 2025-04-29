using System.Text;

namespace Uccs.Fair;

public class ProductUpdation : FairOperation
{
	public AutoId				ProductId { get; set; }
	public string				Name	{ get; set; }
	public byte[]				Value	{ get; set; }
	public override string		Explanation => $"{ProductId}, {Name}, {Value}";

	public override bool		IsValid(McvNet net) => Value.Length <= ProductField.ValueLengthMaximum;

	public ProductUpdation()
	{
	}

	public ProductUpdation(AutoId id)
	{
		ProductId = id;
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
		ProductId	= reader.Read<AutoId>();
		Name		= reader.ReadUtf8();
		Value		= reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(ProductId);
		writer.WriteUtf8(Name);
		writer.WriteBytes(Value);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(RequireProductAccess(execution, ProductId, out var a, out var r) == false)
			return;

		a = execution.AffectAuthor(a.Id);
		r = execution.AffectProduct(ProductId);

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

		foreach(var j in r.Publications)
		{
			var p = execution.AffectPublication(j);
				
			var c = p.Changes.FirstOrDefault(c => c.Name == Name);

			if(c == null)
			{
				p.Changes = [..p.Changes, new ProductFieldVersionReference {Name = Name, Version = f.Versions.Last().Version}];
			} 
			else
			{
				p.Changes = [..p.Changes.Where(i => i != c), new ProductFieldVersionReference {Name = Name, Version = f.Versions.Last().Version}];
			}
		}

		r.Updated = execution.Time;
	}
}