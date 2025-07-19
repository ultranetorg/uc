using System.Text;

namespace Uccs.Fair;

public class ProductUpdation : FairOperation
{
	public AutoId				Product { get; set; }
	public ProductFieldName		Field	{ get; set; }
	public byte[]				Value	{ get; set; }

	public override string		Explanation => $"{Product}, {Field}={Value.ToHex()}";


	public ProductUpdation()
	{
	}

	public ProductUpdation(AutoId id)
	{
		Product = id;
	}

	public override bool IsValid(McvNet net)
	{
		if(Value.Length > ProductField.ValueLengthMaximum)
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Product		= reader.Read<AutoId>();
		Field		= reader.Read<ProductFieldName>();
		Value		= reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.Write(Field);
		writer.WriteBytes(Value);
	}

	public override void Execute(FairExecution execution)
	{
		var v = Value;

		if(CanAccessProduct(execution, Product, out var a, out var r, out Error) == false)
			return;

		a = execution.Authors.Affect(a.Id);
		r = execution.Products.Affect(Product);

		if(Uccs.Fair.Product.IsFile(Field))
		{
			var x = execution.Files.Create(Product);
			x.Data = v;
			v = x.Id.Raw;
		}

		var f = r.Fields.FirstOrDefault(j => j.Name == Field);

		if(f == null)
		{
			f = new ProductField {Name = Field, Versions = [new ProductFieldVersion {Value = v, Version = 0}]};
			r.Fields = [..r.Fields, f];
		}
		else
		{
			f = new ProductField {Name = f.Name, Versions = f.Versions};
			r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];

			if(f.Versions.Last().Refs == 0)
			{
				if(Uccs.Fair.Product.IsFile(Field))
				{
					var fid = new AutoId();
					fid.Read(new BinaryReader(new MemoryStream(f.Versions.Last().Value)));
					var p = execution.Files.Affect(fid);

					p.Deleted = true;
				}

				execution.Free(a, a, f.Versions.Last().Value.Length);

				f.Versions = [..f.Versions[..^1], new ProductFieldVersion {Value = v, Version = f.Versions.Last().Version}];
			}
			else
				f.Versions = [..f.Versions, new ProductFieldVersion {Value = v, Version = f.Versions.Length}];
		}

		execution.Allocate(a, a, v.Length);

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