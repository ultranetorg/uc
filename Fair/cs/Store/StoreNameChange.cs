using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class StoreNameChange : VotableOperation
{
	public string				Name { get; set; }

	public override bool		IsValid(McvNet net) => IsNameValid(Name);
	public override string		Explanation => $"{Name}";

	public StoreNameChange()
	{
	}

	public override void Read(Reader reader)
	{
		Name	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Name);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return false;
	}

	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		error = null;
		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var e = execution.Words.Find(Word.GetId(Name));

		if(e != null)
		{
			Error = NotAvailable;
			return;
		}

		if(Store.Name != null)
		{
			execution.Words.Unregister(Store.Name);
		}

		if(Name != null)
		{
			execution.Words.Register(Name, EntityTextField.StoreName, Store.Id);
		}

		Store.Name = Name;	
	}
}
