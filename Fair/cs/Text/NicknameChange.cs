using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class NicknameChange : VotableOperation
{
	public string				Nickname { get; set; }
	public EntityTextField		Field { get; set; }
	public AutoId				Entity { get; set; }

	public override bool		IsValid(McvNet net) => (Field == EntityTextField.AccountNickname || Field == EntityTextField.AuthorNickname || Field == EntityTextField.SiteNickname) 
														&& Nickname.Length <= 32 
														&& Nickname.Length >= 4 
														&& Regex.Match(Nickname, "^[a-z0-9]+$").Success;
	public override string		Explanation => $"{Field}, {Entity}, {Nickname}";

	public NicknameChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Nickname	= reader.ReadUtf8();
		Field		= reader.Read<EntityTextField>();
		Entity		= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Nickname);
		writer.Write(Field);
		writer.Write(Entity);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as NicknameChange;
		
		return o.Entity == Entity && o.Nickname == Nickname;
	}

 	public override bool ValidProposal(FairExecution execution)
 	{
		if(Field == EntityTextField.AccountNickname	&& !RequireAccountAccess(execution, Entity, out _)) return false;
		if(Field == EntityTextField.AuthorNickname	&& !RequireAuthorAccess(execution, Entity, out _)) return false;
		if(Field == EntityTextField.SiteNickname	&& !RequireSite(execution, Entity, out _)) return false;

		return true;
 	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute && Field == EntityTextField.SiteNickname)
	 	{
	 		if(!RequireModeratorAccess(execution, Entity, out var x))
 				return;

	 		if(x.ChangePolicies[FairOperationClass.NicknameChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
			
			PayEnergyBySite(execution, Entity);	
		}

		//var id = Ngram.GetId(Nickname);
		var e = execution.Words.Find(Word.GetId(Nickname))?.References.FirstOrDefault(i => i.Field == EntityTextField.AccountNickname || i.Field == EntityTextField.AuthorNickname || i.Field == EntityTextField.SiteNickname);

		if(e != null)
		{
			if(e.Entity != Entity)
			{
				Error = NotAvailable;
				return;
			}

			if(e.Field != Field)
				execution.Words.Unregister(Nickname, e.Field, e.Entity);
		}

		if(Nickname != "")
		{
			execution.Words.Register(Nickname, Field, Entity);
		}

		switch(Field)
		{
			case EntityTextField.AccountNickname:	
				execution.AffectAccount(Entity).Nickname = Nickname;	
				break;

			case EntityTextField.AuthorNickname:	
				execution.Authors.Affect(Entity).Nickname = Nickname;	
				break;
			
			case EntityTextField.SiteNickname:		
				execution.Sites.Affect(Entity).Nickname = Nickname;	
				break;
		}
	}
}
