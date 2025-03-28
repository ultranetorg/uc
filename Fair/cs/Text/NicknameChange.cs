namespace Uccs.Fair;

public class NicknameChange : VotableOperation
{
	public string				Nickname { get; set; }
	public EntityTextField		Field { get; set; }
	public EntityId				Entity { get; set; }

	public override bool		IsValid(McvNet net) => (Field == EntityTextField.AccountNickname || 
														Field == EntityTextField.AuthorNickname || 
														Field == EntityTextField.ProductNickname || 
														Field == EntityTextField.SiteNickname) && Nickname.Length <= 32;
	public override string		Description => $"{Nickname}";

	public NicknameChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Nickname		= reader.ReadUtf8();
		Field		= reader.Read<EntityTextField>();
		Entity		= reader.Read<EntityId>();
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
		if(Field == EntityTextField.ProductNickname	&& !RequireProductAccess(execution, Entity, out _, out _)) return false;
		if(Field == EntityTextField.SiteNickname	&& !RequireSite(execution, Entity, out _)) return false;

		return true;
 	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute && Field == EntityTextField.SiteNickname)
	 	{
	 		if(!RequireSiteModeratorAccess(execution, Entity, out var x))
 				return;

	 		if(x.ChangePolicies[FairOperationClass.NicknameChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}

		var t = execution.FindText(new StringId(Nickname));
		var e = t?.Entities.FirstOrDefault(i => i.Field == EntityTextField.AccountNickname || i.Field == EntityTextField.AuthorNickname || i.Field == EntityTextField.ProductNickname || i.Field == EntityTextField.SiteNickname);


		if(e != null)
		{
			if(e.Entity != Entity)
			{
				Error = NotAvailable;
				return;
			}

			if(e.Field != Field)
				t.Entities = t.Entities.Remove(e);
		}

		if(Nickname != "")
		{
			t = t == null ? execution.CreateText(Nickname) : execution.AffectText(new StringId(Nickname));
			
			e = new TextField {Entity = Entity, Field = Field};
				
			t.Entities = [..t.Entities, e];
		}

		switch(Field)
		{
			case EntityTextField.AccountNickname:	execution.AffectAccount(Entity).Nickname = Nickname;	break;
			case EntityTextField.AuthorNickname:	execution.AffectAuthor(Entity).Nickname = Nickname;		break;
			case EntityTextField.SiteNickname:		execution.AffectSite(Entity).Nickname = Nickname;		break;
			case EntityTextField.ProductNickname:	execution.AffectProduct(Entity).Nickname = Nickname;	break;
		}
	}
}
