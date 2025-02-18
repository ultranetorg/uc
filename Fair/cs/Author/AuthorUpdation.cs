namespace Uccs.Fair;

public enum AuthorChange : byte
{
	None, Renew, Owner, DepositEnergy, DepositEnergyNext, DepositSpacetime, ModerationReward
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
							AuthorChange.DepositSpacetime	=> reader.Read7BitEncodedInt64(),
							AuthorChange.DepositEnergy		=> reader.Read7BitEncodedInt64(),
							AuthorChange.DepositEnergyNext	=> reader.Read7BitEncodedInt64(),
							AuthorChange.ModerationReward	=> reader.Read7BitEncodedInt64(),
							AuthorChange.Owner				=> reader.Read<EntityId>(),
							_								=> throw new IntegrityException()
					   };

// 		Second = Change switch
// 						{
// 							AuthorChange.Renew				=> reader.Read7BitEncodedInt(),
// 							_								=> throw new IntegrityException()
// 						};
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case AuthorChange.Renew				: writer.Write(Byte); break;
			case AuthorChange.DepositSpacetime	: writer.Write7BitEncodedInt64(Long); break;
			case AuthorChange.DepositEnergy		: writer.Write7BitEncodedInt64(Long); break;
			case AuthorChange.DepositEnergyNext	: writer.Write7BitEncodedInt64(Long); break;
			case AuthorChange.ModerationReward	: writer.Write7BitEncodedInt64(Long); break;
			case AuthorChange.Owner				: writer.Write(EntityId); break;
			default								: throw new IntegrityException();
		}

// 		switch(Change)
// 		{
// 			case AuthorChange.Renew				: writer.Write7BitEncodedInt((int)Second); break;
// 			default								: throw new IntegrityException();
// 		}
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
				if(!Author.CanRenew(a, Signer, (short)round.ConsensusTime.Days))
				{
					Error = NotAvailable;
					return;
				}

				Prolong(round, Signer, a, Time.FromYears(Byte));

				break;
			}

			case AuthorChange.DepositSpacetime:
			{	
				a.Spacetime		 += Long;
				Signer.Spacetime -= Long;

				SpacetimeSpenders.Add(Signer);

				break;
			}

			case AuthorChange.DepositEnergy:
			{	
				a.Energy		+= Long;
				Signer.Energy	-= Long;

				break;
			}

			case AuthorChange.DepositEnergyNext:
			{	
				a.EnergyNext		+= Long;
				Signer.EnergyNext	-= Long;

				break;
			}

			case AuthorChange.ModerationReward:

				a.ModerationReward = Long;

				break;

			case AuthorChange.Owner:
			{
				var x = mcv.Accounts.Find(EntityId, round.Id);

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
