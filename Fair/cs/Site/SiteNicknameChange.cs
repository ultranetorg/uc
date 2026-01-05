using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class SiteNicknameChange : VotableOperation
{
	public string				Nickname { get; set; }

	public override bool		IsValid(McvNet net) =>	Nickname.Length <= 32 
														&& Nickname.Length >= 4 
														&& Regex.Match(Nickname, "^[a-z0-9_]+$").Success;
	public override string		Explanation => $"{Nickname}";

	public SiteNicknameChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Nickname	= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Nickname);
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
		var e = execution.Words.Find(Word.GetId(Nickname));

		if(e != null)
		{
			Error = NotAvailable;
			return;
		}

		if(Site.Nickname != "")
		{
			execution.Words.Unregister(Site.Nickname);
		}

		if(Nickname != "")
		{
			execution.Words.Register(Nickname, EntityTextField.SiteNickname, Site.Id);
		}

		Site.Nickname = Nickname;	
	}
}
