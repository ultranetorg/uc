namespace Uccs.Fair;

public enum AuthorChange : byte
{
	None, Renew, AddOwner, RemoveOwner, ModerationReward
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
							AuthorChange.ModerationReward	=> reader.Read7BitEncodedInt64(),
							AuthorChange.AddOwner			=> reader.Read<EntityId>(),
							AuthorChange.RemoveOwner		=> reader.Read<EntityId>(),
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
			case AuthorChange.ModerationReward	: writer.Write7BitEncodedInt64(Long); break;
			case AuthorChange.AddOwner			: writer.Write(EntityId); break;
			case AuthorChange.RemoveOwner		: writer.Write(EntityId); break;
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

				Prolong(round, Signer, a, Time.FromYears(Byte));

				break;
			}

			case AuthorChange.ModerationReward:

				a.ModerationReward = Long;

				break;

			case AuthorChange.AddOwner:
			{
				if(!RequireAccount(round, EntityId, out var x))
					return;

				if(x.AllocationSponsor != null)
				{
					Error = NotAllowedForFreeAccount;
					return;
				}

				a.Owners = [..a.Owners, x.Id];

				break;;
			}

			case AuthorChange.RemoveOwner:
			{
				if(a.Owners.Length == 1)
				{
					Error = AtLeastOneOwnerRequired;
					return;
				}

				if(!RequireAccount(round, EntityId, out var x))
					return;

				a.Owners = a.Owners.Where(i => i != x.Id).ToArray();

				break;;
			}
		}
	}
}
