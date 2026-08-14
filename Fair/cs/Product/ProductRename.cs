using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class ProductRenaming : FairOperation
{
	public AutoId				Product { get; set; }
	public string				Name { get; set; }

	public override string		Explanation => $"{Product}, {Name}";

	public override bool		IsValid(McvNet net) => Uccs.Net.User.IsNameValid(Name);

	public ProductRenaming()
	{
	}

	public override void Read(Reader reader)
	{
		Product	= reader.Read<AutoId>();
		Name	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Product);
		writer.WriteUtf8(Name);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessProduct(execution, Product, out var a, out var p, out Error))
			return;

		var e = execution.Names.Find(NameIndex.GetId(Name));

		if(e != null)
		{
			Error = NotAvailable;
			return;
		}

		p = execution.Products.Affect(p.Id);
		a = execution.Authors.Affect(a.Id);

		if(p.Name != null)
		{
			execution.Names.Unregister(p.Name);
			execution.Free(a, a, execution.Net.EntityLength);
		}

		if(Name != null)
		{
			execution.Names.Register(Name, EntityTextField.ProductName, p.Id);
			execution.Allocate(a, a, execution.Net.EntityLength);
		}
		
		p.Name = Name;	
		
		execution.PayOperationEnergy(a);
	}
}
