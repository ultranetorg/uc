using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class AuthorNameChange : FairOperation
{
	public AutoId				Author { get; set; }
	public string				Name { get; set; }

	public override bool		IsValid(McvNet net) => IsNameValid(Name);
	public override string		Explanation => $"{Author}, {Name}";

	public AuthorNameChange()
	{
	}

	public override void Read(Reader reader)
	{
		Author	= reader.Read<AutoId>();
		Name	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Author);
		writer.WriteUtf8(Name);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		var e = execution.Words.Find(Word.GetId(Name));

		if(e != null)
		{
			Error = NotAvailable;
			return;
		}

		a = execution.Authors.Affect(Author);

		if(a.Name != null)
		{
			execution.Words.Unregister(a.Name);
			execution.Free(a, a, execution.Net.EntityLength);
		}

		if(Name != null)
		{
			execution.Words.Register(Name, EntityTextField.AuthorName, a.Id);
			execution.Allocate(a, a, execution.Net.EntityLength);
		}
		
		a.Name = Name;	
		
		execution.PayOperationEnergy(a);
	}
}
