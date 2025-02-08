namespace Uccs.Fair;

public enum AuthorChange : byte
{
	None, Renew, Owner, DepositEC, DepositBY, ModerationReward
}

public class AuthorUpdation : UpdateOperation
{
	public EntityId				AuthorId { get; set; }
	public AuthorChange			Change { get; set; }
	public object				Second { get; set; }

	public override string		Description => $"{AuthorId}, {Change}, {Value}";
	
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
		AuthorId	= reader.Read<EntityId>();
		Change		= reader.ReadEnum<AuthorChange>();

		Value = Change switch
					   {
							AuthorChange.Renew				=> reader.ReadByte(),
							AuthorChange.Owner				=> reader.Read<AccountAddress>(),
							AuthorChange.DepositEC			=> reader.Read7BitEncodedInt(),
							AuthorChange.ModerationReward	=> reader.Read7BitEncodedInt(),
							_								=> throw new IntegrityException()
					   };

		Second = Change switch
						{
							AuthorChange.Renew				=> reader.Read7BitEncodedInt(),
							_								=> throw new IntegrityException()
						};
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case AuthorChange.Renew				: writer.Write(Byte); break;
			case AuthorChange.Owner				: writer.Write(AccountAddress); break;
			case AuthorChange.DepositEC			: writer.Write7BitEncodedInt(Int); break;
			case AuthorChange.ModerationReward	: writer.Write7BitEncodedInt(Int); break;
			default								: throw new IntegrityException();
		}

		switch(Change)
		{
			case AuthorChange.Renew				: writer.Write7BitEncodedInt((int)Second); break;
			default								: throw new IntegrityException();
		}
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireAuthorAccess(round, AuthorId, out var a))
			return;
		
		a = round.AffectAuthor(AuthorId);

		switch(Change)
		{
			case AuthorChange.Renew:
			{	
				if(!Author.CanRenew(a, Signer, round.ConsensusTime))
				{
					Error = NotAvailable;
					return;
				}

				//if()
				//{
				//}

				a.SpaceReserved	= a.SpaceUsed;
				a.Expiration	= a.Expiration + Time.FromYears(Byte);
				
				PayForSpacetime(a.SpaceUsed, Time.FromYears(Byte));

				break;
			}

			case AuthorChange.DepositEC:
			{	
				//if(!Pay(ref Signer.ECBalance, ref a.ECDeposit, Int, round.ConsensusTime))
				//	return;

				break;
			}

// 			case AuthorChange.DepositBY:
// 			{	
// 				if(Signer.BYBalance < Int)
// 				{
// 					Error = NotEnoughBY;
// 					return;
// 				}
// 
// 				Signer.BYBalance -= Int;
// 				a.BYDeposit		 += Int;
// 
// 				break;
// 			}

			case AuthorChange.ModerationReward:
				a.ModerationReward = Int;
				break;

			case AuthorChange.Owner:
			{
				var x = mcv.Accounts.Find(AccountAddress, round.Id);

				if(x == null)
				{
					Error = NotFound;
					return;
				}

				a.Owner	= x.Id;

				break;;
			}
		}
	}
}
