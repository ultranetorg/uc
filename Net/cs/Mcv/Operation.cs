namespace Uccs.Net;

public struct Portion
{
	public Unit	Factor;
	public Unit	Amount;
}

public enum OperationClass
{
	None = 0, 
	AccountCreation				= 000_000_001, 
	CandidacyDeclaration		= 000_000_002, 
	UtilityTransfer				= 000_000_003,
	BandwidthAllocation			= 000_000_004,

	ChildNet					= 001, 
		ChildNetInitialization	= 001_000_001,
}


public abstract class Operation : ITypeCode, IBinarySerializable
{
	public string						Error;
	public Transaction					Transaction;
	public Account					Signer;
	public IEnergyHolder				EnergyFeePayer;
	public HashSet<IEnergyHolder>		EnergySpenders;
	public HashSet<ISpacetimeHolder>	SpacetimeSpenders;
	public abstract string				Description { get; }
	public long							EnergyConsumed;

	public virtual bool					NonExistingSignerAllowed => false;

	public const string					AlreadyExists = "Already exists";
	public const string					AtLeastOneOwnerRequired = "At least one owner required";
	public const string					Denied = "Access denied";
	public const string					ExistingAccountRequired = "ExistingAccountRequired";
	public const string					Expired = "Expired";
	public const string					LimitReached = "Limit Reached";
	public const string					Mismatch = "Mismatch";
	public const string					NotAvailable = "Not Available";
	public const string					NotFound = "Not found";
	public const string					NotSequential = "Not sequential";
	public const string					NotEnergyHolder = "Not Energy Holder";
	public const string					NotEnoughSpacetime = "Not enough spacetime";
	public const string					NotEnoughEnergy = "Not enough execution units";
	public const string					NotEnoughEnergyNext = "Not enough energy for next period";
	public const string					NotEnoughBandwidth = "Not enough bandwidth";
	public const string					NotSpacetimeHolder = "Not spacetime holder";
	public const string					NothingLastCreated = "Nothing last created";
	public const string					Rejected = "Rejected";

	protected OperationId				_Id;
	
	public OperationId Id
	{
		get
		{
			if(_Id == default)
			{
				_Id = new (Transaction.Id.Ri, Transaction.Id.Ti, (byte)Array.IndexOf(Transaction.Operations, this));
			}

			return _Id;
		}
	}

	public Operation()
	{
	}
	
	public abstract bool IsValid(McvNet net);
	public abstract void Execute(Execution execution);
	public abstract void Write(BinaryWriter w);
	public abstract void Read(BinaryReader r);
	 
	public override string ToString()
	{
		return $"{GetType().Name}, {Description}{(Error == null ? null : ", Error=" + Error)}";
	}

	public Account RequireAccount(Round round, AccountAddress account)
	{
		var a = round.Mcv.Accounts.Find(account, round.Id);

		if(a == null || a.Deleted)
		{
			Error = NotFound;
			return a;
		}

		return a;
	}

	public static long ToBD(long length, short time)
	{
		return time * length;
	}

	public static long ToBD(long length, Time time)
	{
		return time.Days * length;
	}

	public void Prolong(Execution execution, ISpacetimeHolder payer, ISpaceConsumer consumer, Time duration)
	{	
		var start = (short)(consumer.Expiration < execution.Time.Days ? execution.Time.Days : consumer.Expiration);

		consumer.Expiration = (short)(start + duration.Days);

		if(consumer.Space > 0)
		{
			payer.Spacetime -= ToBD(consumer.Space, duration);
			SpacetimeSpenders.Add(payer);
		}

		var n = start + duration.Days - execution.Time.Days;

		if(n > execution.Spacetimes.Length)
			execution.Spacetimes = [..execution.Spacetimes, ..new long[n - execution.Spacetimes.Length]];

		for(int i = 0; i < duration.Days; i++)
			execution.Spacetimes[start - execution.Time.Days + i] += consumer.Space;

	}

	public void AllocateEntity(ISpacetimeHolder payer)
	{
		payer.Spacetime -= ToBD(Transaction.Net.EntityLength, Mcv.Forever);
		SpacetimeSpenders.Add(payer);
	}

	public void FreeEntity(Execution round)
	{
		round.Spacetimes[0] += ToBD(Transaction.Net.EntityLength, Mcv.Forever); /// to be distributed between members
	}

	public void Allocate(Execution round, ISpacetimeHolder payer, ISpaceConsumer consumer, int space)
	{
		if(space == 0)
			return;

		consumer.Space += space;

		var n = consumer.Expiration - round.Time.Days;
	
		payer.Spacetime -= ToBD(space, (short)n);
		SpacetimeSpenders.Add(payer);

		for(int i = 0; i < n; i++)
			round.Spacetimes[i] += space;
	}

	public void Free(Execution round, ISpacetimeHolder beneficiary, ISpaceConsumer consumer, long space)
	{
		if(space == 0)
			return;

		consumer.Space -= space;

		if(consumer.Space < 0)
			throw new IntegrityException();

		var d = consumer.Expiration - round.Time.Days;
		
		if(d > 0)
		{
			beneficiary.Spacetime += ToBD(space, (short)(d - 1));
	
			for(int i = 1; i < d; i++)
				round.Spacetimes[i] -= space;
		}
	}

	public bool RequireAccount(Execution round, EntityId id, out Account account)
	{
		account = round.FindAccount(id);

		if(account == null || account.Deleted)
		{
			Error = NotFound;
			return false;
		}

		return true;
	}

	public bool RequireAccountAccess(Execution round, EntityId id, out Account account)
	{
		if(!RequireAccount(round, id, out account))
			return false;

		if(account.Address != Signer.Address)
		{
			Error = Denied;
			return false;
		}

		return true;
	}
}
