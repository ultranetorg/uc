using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class AuthorNicknameChange : FairOperation
{
	public AutoId				Author { get; set; }
	public string				Nickname { get; set; }

	public override bool		IsValid(McvNet net) =>	Nickname.Length <= 32 
														&& Nickname.Length >= 4 
														&& Regex.Match(Nickname, "^[a-z0-9]+$").Success;
	public override string		Explanation => $"{Author}, {Nickname}";

	public AuthorNicknameChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Nickname	= reader.ReadUtf8();
		Author		= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Nickname);
		writer.Write(Author);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		var e = execution.Words.Find(Word.GetId(Nickname));

		if(e != null)
		{
			Error = NotAvailable;
			return;
		}

		if(a.Nickname != "")
		{
			execution.Words.Unregister(a.Nickname);
		}

		if(Nickname != "")
		{
			execution.Words.Register(Nickname, EntityTextField.AuthorNickname, a.Id);
		}

		a = execution.Authors.Affect(Author);
		
		a.Nickname = Nickname;	
		
		execution.PayCycleEnergy(a);
	}
}
