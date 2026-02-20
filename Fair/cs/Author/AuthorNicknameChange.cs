using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class AuthorNicknameChange : FairOperation
{
	public AutoId				Author { get; set; }
	public string				Name { get; set; }

	public override bool		IsValid(McvNet net) => IsFreeNameValid(Name);
	public override string		Explanation => $"{Author}, {Name}";

	public AuthorNicknameChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Author	= reader.Read<AutoId>();
		Name	= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
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

		if(a.Name != null)
		{
			execution.Words.Unregister(a.Name);
		}

		if(Name != null)
		{
			execution.Words.Register(Name, EntityTextField.AuthorName, a.Id);
		}

		a = execution.Authors.Affect(Author);
		
		a.Name = Name;	
		
		execution.PayOperationEnergy(a);
	}
}
