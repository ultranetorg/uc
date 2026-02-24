using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class SiteNameChange : VotableOperation
{
	public string				Name { get; set; }

	public override bool		IsValid(McvNet net) => IsFreeNameValid(Name);
	public override string		Explanation => $"{Name}";

	public SiteNameChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Name	= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
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

		if(Site.Name != null)
		{
			execution.Words.Unregister(Site.Name);
		}

		if(Name != null)
		{
			execution.Words.Register(Name, EntityTextField.SiteName, Site.Id);
		}

		Site.Name = Name;	
	}
}
