using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class AccountNicknameChange : FairOperation
{
	public string				Nickname { get; set; }

	public override bool		IsValid(McvNet net) =>	Nickname.Length <= 32 
														&& Nickname.Length >= 4 
														&& Regex.Match(Nickname, "^[a-z0-9]+$").Success;
	public override string		Explanation => $"{Nickname}";

	public AccountNicknameChange()
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

	public override void Execute(FairExecution execution)
	{
		var e = execution.Words.Find(Word.GetId(Nickname))?.References.FirstOrDefault(i => i.Field == EntityTextField.AccountNickname || i.Field == EntityTextField.AuthorNickname || i.Field == EntityTextField.SiteNickname);

		if(e != null)
		{
			if(e.Entity != Signer.Id)
			{
				Error = NotAvailable;
				return;
			}

			if(e.Field != EntityTextField.AccountNickname)
				execution.Words.Unregister(Nickname, e.Field, e.Entity);
		}

		if(Nickname != "")
		{
			execution.Words.Register(Nickname, EntityTextField.AccountNickname, Signer.Id);
		}

		Signer.Nickname = Nickname;	
		
		execution.PayCycleEnergy(Signer);
	}
}
