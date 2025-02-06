namespace Uccs.Fair;

public enum AuthorChange : byte
{
	None, Renew, Owner, Deposit, ModerationReward
}

public class AuthorUpdation : FairOperation
{
	public new EntityId			Id { get; set; }
	public AuthorChange			Change { get; set; }
	public object				Value { get; set; }

	protected string[]			Strings  => Value as string[];
	protected EntityId			EntityId  => Value as EntityId;
	protected AccountAddress	AccountAddress  => Value as AccountAddress;
	protected byte				Byte => (byte)Value;
	protected long				Long => (long)Value;
	protected int				Int	=> (int)Value;

	public override string		Description => $"{Id}, {Change}, {Value}";
	
	public AuthorUpdation()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Enum.IsDefined(Change) || Change == AuthorChange.None) 
			return false;
		
		if(Change == AuthorChange.Renew && (Byte < Mcv.EntityRentYearsMin || Byte > Mcv.EntityRentYearsMax))
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Change	= reader.ReadEnum<AuthorChange>();

		Value = Change switch
					   {
							AuthorChange.Renew				=> reader.ReadByte(),
							AuthorChange.Owner				=> reader.Read<AccountAddress>(),
							AuthorChange.Deposit			=> reader.Read7BitEncodedInt64(),
							AuthorChange.ModerationReward	=> reader.Read7BitEncodedInt(),
							_								=> throw new IntegrityException()
					   };
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case AuthorChange.Renew				: writer.Write(Byte); break;
			case AuthorChange.Owner				: writer.Write(AccountAddress); break;
			case AuthorChange.Deposit			: writer.Write7BitEncodedInt64(Long); break;
			case AuthorChange.ModerationReward	: writer.Write7BitEncodedInt(Int); break;
			default								: throw new IntegrityException();
		}
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireAuthorAccess(round, Id, out var a))
			return;
		
		a = round.AffectAuthor(Id);

		if(Change == AuthorChange.Renew)
		{	
			if(!Author.CanRenew(a, Signer, round.ConsensusTime))
			{
				Error = NotAvailable;
				return;
			}

			a.SpaceReserved	= a.SpaceUsed;
			a.Expiration = a.Expiration + Time.FromYears(Byte);
				
			PayForSpacetime(a.SpaceUsed, Time.FromYears(Byte));
		}
		else if(Change == AuthorChange.Deposit)
		{	
			if(EC.Integrate(Signer.ECBalance, round.ConsensusTime) - Long < 0)
			{
				Error = NotEnoughEC;
				return;
			}

			var d			 = EC.TakeOldest(Signer.ECBalance, Long, round.ConsensusTime);
			Signer.ECBalance = EC.Subtract(Signer.ECBalance, Long, round.ConsensusTime);
			a.ECDeposit		 = EC.Add(a.ECDeposit, d);
		}
		else if(Change == AuthorChange.ModerationReward)
		{	
			a.ModerationReward = Int;
		}
		else if(Change == AuthorChange.Owner)
		{
			if(!Author.IsOwner(a, Signer, round.ConsensusTime))
			{
				Error = Denied;
				return;
			}

			a.Owner	= round.AffectAccount(AccountAddress).Id;
		}
	}
}
