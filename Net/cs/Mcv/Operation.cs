namespace Uccs.Net;

public struct Portion
{
	public Unit	Factor;
	public Unit	Amount;
}

public enum OperationClass
{
	None = 0, 
	CandidacyDeclaration		= 000_000_001, 
	UtilityTransfer				= 000_000_002,
	BandwidthAllocation			= 000_000_003,

	ChildNet					= 001, 
		ChildNetInitialization	= 001_000_001,
}


public abstract class Operation : ITypeCode, IBinarySerializable
{
	public string			Error;
	public Transaction		Transaction;
	public AccountEntry		Signer;
	public IEnergyHolder	EnergySource;
	public ISpaceHolder		SpacetimeSource;
	public abstract string	Description { get; }
	public long				EnergyConsumed;

	public const string		Rejected = "Rejected";
	public const string		NotFound = "Not found";
	public const string		NotAvailable = "Not Available";
	public const string		Mismatch = "Mismatch";
	public const string		ExistingAccountRequired = "ExistingAccountRequired";
	public const string		Expired = "Expired";
	public const string		Sealed = "Sealed";
	public const string		NotSealed = "NotSealed";
	public const string		NoData = "NoData";
	public const string		AlreadyExists = "Already exists";
	public const string		NotSequential = "Not sequential";
	public const string		NotEnoughSpacetime = "Not enough spacetime";
	public const string		NotEnoughEnergy = "Not enough execution units";
	public const string		NotEnoughEnergyNext = "Not enough energy for next period";
	public const string		NotEnoughBandwidth = "Not enough bandwidth";
	public const string		NoAnalyzers = "No analyzers";
	public const string		Denied = "Access denied";
	public const string		NotRelease = "Data valus is not a release";
	public const string		LimitReached = "Limit Reached";

	protected OperationId	_Id;
	
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
	
	static Operation()
	{
	}

	public Operation()
	{
	}
	
	public abstract bool IsValid(Mcv mcv);
	public abstract void Execute(Mcv mcv, Round round);
	public abstract void WriteConfirmed(BinaryWriter w);
	public abstract void ReadConfirmed(BinaryReader r);
	 
	public override string ToString()
	{
		return $"{GetType().Name}, {Description}{(Error == null ? null : ", Error=" + Error)}";
	}

	public void Read(BinaryReader reader)
	{
		ReadConfirmed(reader);
	}

	public void Write(BinaryWriter writer)
	{
		WriteConfirmed(writer);
	}

	public AccountEntry RequireAccount(Round round, AccountAddress account)
	{
		var a = round.Mcv.Accounts.Find(account, round.Id);

		if(a == null)
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

	public void Prolong(Round round, ISpaceHolder payer, ISpaceConsumer consumer, short duration)
	{
		var start = (short)(consumer.Expiration < round.ConsensusTime.Days ? round.ConsensusTime.Days : consumer.Expiration);

		consumer.Expiration = (short)(start + duration);

		if(consumer.Space == 0)
			return;

		payer.Spacetime -= ToBD(consumer.Space, duration);

		var n = start + duration - round.ConsensusTime.Days;

		if(n > round.Spacetimes.Length)
			round.Spacetimes = [..round.Spacetimes, ..new long[n - round.Spacetimes.Length]];

		for(int i = 0; i < duration; i++)
			round.Spacetimes[start - round.ConsensusTime.Days + i] += consumer.Space;
	}

	public void AllocateEntity(ISpaceHolder payer)
	{
		payer.Spacetime -= ToBD(Mcv.EntityLength, Mcv.Forever);
	}

	public void FreeEntity(Round round)
	{
		round.Spacetimes[0] += ToBD(Mcv.EntityLength, Mcv.Forever);
	}

	public void Allocate(Round round, ISpaceHolder payer, ISpaceConsumer consumer, int space)
	{
		consumer.Space += space;

		var n = consumer.Expiration - round.ConsensusTime.Days;
	
		payer.Spacetime -= ToBD(space, (short)n);

		if(n > round.Spacetimes.Length)
			round.Spacetimes = [..round.Spacetimes, ..new long[n - round.Spacetimes.Length]];

		for(int i = 0; i < n; i++)
			round.Spacetimes[i] += space;
	}

	public void Free(Round round, ISpaceHolder beneficiary, ISpaceConsumer consumer, long space)
	{
		if(space == 0)
			return;

		consumer.Space -= space;

		var d = consumer.Expiration - round.ConsensusTime.Days;
		
		if(d > 0)
		{
			beneficiary.Spacetime += ToBD(space, (short)(d - 1));
	
			for(int i = 1; i < d; i++)
				round.Spacetimes[i] -= space;
		}
	}
}
