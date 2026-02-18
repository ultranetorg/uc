using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class SiteNicknameChange : VotableOperation
{
	public string				Name { get; set; }

	public override bool		IsValid(McvNet net) => IsFreeNameValid(Name);
	public override string		Explanation => $"{Name}";

	public SiteNicknameChange()
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

		if(Site.Nickname != null)
		{
			execution.Words.Unregister(Site.Nickname);
		}

		if(Name != null)
		{
			execution.Words.Register(Name, EntityTextField.SiteNickname, Site.Id);
		}

		Site.Nickname = Name;	
	}
}
